using Microsoft.Extensions.Options;

namespace ShikicinemaStatics.Posters;

internal sealed class PostersLoader : IHostedService, IDisposable
{
    private readonly IOptionsMonitor<PostersLoaderOptions> _monitor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PostersLoader> _logger;

    private CancellationTokenSource? _cts;
    private Task? _task;
    private IDisposable? _onOptionsChange;

    public PostersLoader(IServiceProvider serviceProvider,
        IOptionsMonitor<PostersLoaderOptions> monitor,
        ILogger<PostersLoader> logger)
    {
        _serviceProvider = serviceProvider;
        _monitor = monitor;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();
        _task = LoadPostersAsync(_monitor.CurrentValue, _cts.Token);

        _onOptionsChange = _monitor.OnChange(options =>
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _task = LoadPostersAsync(options, _cts.Token);
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        return _task ?? Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts?.Dispose();
        _onOptionsChange?.Dispose();
    }

    private async Task LoadPostersAsync(PostersLoaderOptions options, CancellationToken token)
    {
        if (!options.Enabled)
        {
            _logger.LogInformation("Posters loading is disabled {Instance}", options.GetHashCode());
            return;
        }

        _logger.LogInformation("Posters loading has started {Instance}", options.GetHashCode());

        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await LoadBatchesAsync(options, token);
                }
                catch (OperationCanceledException e) when (e.CancellationToken == token)
                {
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Posters loading has failed");
                }

                _logger.LogInformation("Posters loading has finished. Sleep for {ScanInterval}", options.ScanInterval.ToString());
                await Task.Delay(options.ScanInterval, token);
            }
        }
        catch (OperationCanceledException e) when (e.CancellationToken == token)
        {
        }

        _logger.LogInformation("Posters loading has stopped {Instance}", options.GetHashCode());
    }

    private async Task LoadBatchesAsync(PostersLoaderOptions options, CancellationToken token)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var posterListProvider = scope.ServiceProvider.GetRequiredService<IPosterListProvider>();
        var httpFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var store = scope.ServiceProvider.GetRequiredService<IPosterStore>();

        var page = 0;
        while (!token.IsCancellationRequested)
        {
            page++;
            var posters = await posterListProvider.GetPostersAsync(page, cancellationToken: token);

            if (!posters.Any()) return;

            var http = httpFactory.CreateClient(nameof(PostersLoader));
            foreach (var poster in posters)
            {
                var bytes = await http.GetByteArrayAsync(poster.OriginalUrl, token);
                await store.SavePosterAsync(poster.AnimeId, bytes, "jpeg");

                bytes = await http.GetByteArrayAsync(poster.MainUrl, token);
                await store.SavePosterAsync(poster.AnimeId, bytes, "webp");
            }

            if (options.QueriesInterval > TimeSpan.Zero) await Task.Delay(options.QueriesInterval, token);
        }
    }
}
