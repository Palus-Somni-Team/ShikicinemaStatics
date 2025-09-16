using Microsoft.Extensions.Options;
using ShikicinemaStatics.Posters.PosterListProviders;

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
            using var pageLogScope = _logger.BeginScope(new Dictionary<string, object> { ["Page"] = page });

            _logger.LogInformation("Loading posters page");
            var posters = await DoWithInfinityRetriesAsync(
                () => posterListProvider.GetPostersAsync(page, cancellationToken: token),
                "Loading posters page"
            );

            var hasPosters = false;
            foreach (var poster in posters)
            {
                using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["AnimeId"] = poster.AnimeId });
                _logger.LogInformation("Loading poster");
                if (!string.IsNullOrEmpty(poster.OriginalUrl))
                {
                    var bytes = await DoWithInfinityRetriesAsync(
                        () => http.GetByteArrayAsync(poster.OriginalUrl, token),
                        "Loading poster"
                    );

                    await DoWithInfinityRetriesAsync(
                        async () =>
                        {
                            await store.SavePosterAsync(poster.AnimeId, bytes);
                            return true;
                        },
                        "Poster saving"
                    );

                    if (options.QueriesInterval > TimeSpan.Zero) await Task.Delay(options.QueriesInterval, token);
                }

                _logger.LogInformation("Poster has been loaded");
                hasPosters = true;
            }

            _logger.LogInformation("Posters page has been loaded");
            if (!hasPosters) break;
        }
    }

    private async Task<T> DoWithInfinityRetriesAsync<T>(Func<Task<T>> action, string actionName)
    {
        var pause = TimeSpan.FromSeconds(1);
        var maxPause = TimeSpan.FromMinutes(20);
        while (true)
        {
            try
            {
                var result = await action();
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "InfinityRetries failed for {ActionName}. Sleep for {Pause}", pause, actionName);
                await Task.Delay(pause);
                pause = pause >= maxPause ? maxPause : pause + pause;
            }
        }
    }
}
