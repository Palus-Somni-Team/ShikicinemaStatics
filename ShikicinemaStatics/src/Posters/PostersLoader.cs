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
    private PostersLoaderOptions? _currentOptions;

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
        _currentOptions = _monitor.CurrentValue;
        _task = LoadPostersAsync(_monitor.CurrentValue, _cts.Token);

        _onOptionsChange = _monitor.OnChange(options =>
        {
            if (_currentOptions == options) return;
            _currentOptions = options;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _task.Wait(_cts.Token);
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
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["Instance"] = options.GetHashCode() });

        if (!options.Enabled)
        {
            _logger.LogInformation("Posters loading is disabled");
            return;
        }

        _logger.LogInformation("Posters loading has started");

        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await LoadBatchesAsync(options, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Posters loading has failed");
                }

                _logger.LogInformation("Posters loading has finished. Sleep for {ScanInterval}", options.ScanInterval.ToString());
                await Task.Delay(options.ScanInterval, token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        _logger.LogInformation("Posters loading has stopped");
    }

    private async Task LoadBatchesAsync(PostersLoaderOptions options, CancellationToken token)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var posterListProvider = scope.ServiceProvider.GetRequiredService<IPosterListProvider>();
        var httpFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var store = scope.ServiceProvider.GetRequiredService<IPosterStore>();
        var http = httpFactory.CreateClient(nameof(PostersLoader));

        var page = 0;
        while (!token.IsCancellationRequested)
        {
            page++;

            _logger.LogInformation("Loading posters page {Page}", page);
            var posters = await posterListProvider.GetPostersAsync(page, cancellationToken: token);

            var hasPosters = false;
            foreach (var poster in posters)
            {
                _logger.LogInformation("Loading posters for {AnimeId}", poster.AnimeId);
                if (options.QueriesInterval > TimeSpan.Zero) await Task.Delay(options.QueriesInterval, token);
                var bytes = await http.GetByteArrayAsync(poster.OriginalUrl, token);
                await store.SavePosterAsync(poster.AnimeId, bytes, "jpeg");

                if (options.QueriesInterval > TimeSpan.Zero) await Task.Delay(options.QueriesInterval, token);
                bytes = await http.GetByteArrayAsync(poster.MainUrl, token);
                await store.SavePosterAsync(poster.AnimeId, bytes, "webp");

                _logger.LogInformation("Posters for {AnimeId} has been loaded", poster.AnimeId);
                hasPosters = true;
            }

            _logger.LogInformation("Posters page {Page} has been loaded", page);
            if (!hasPosters) break;
        }
    }
}
