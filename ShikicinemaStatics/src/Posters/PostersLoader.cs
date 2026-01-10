using Microsoft.Extensions.Options;
using ShikicinemaStatics.Posters.PosterListProviders;

namespace ShikicinemaStatics.Posters;

internal sealed class PostersLoader : IPostersLoader
{
    private readonly ILogger<PostersLoader> _logger;
    private readonly IPosterListProvider _posterListProvider;
    private readonly IPosterStore _posterStore;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PostersLoaderOptions _options;

    public PostersLoader(IPosterListProvider posterListProvider,
        IPosterStore posterStore,
        IHttpClientFactory httpClientFactory,
        IOptionsSnapshot<PostersLoaderOptions> options,
        ILogger<PostersLoader> logger)
    {
        _posterListProvider = posterListProvider;
        _posterStore = posterStore;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task LoadPostersAsync(CancellationToken token = default)
    {
        using var http = _httpClientFactory.CreateClient(nameof(PostersLoader));

        var page = 0;
        while (!token.IsCancellationRequested)
        {
            page++;
            using var pageLogScope = _logger.BeginScope(new Dictionary<string, object> { ["Page"] = page });

            var posters = await _posterListProvider.GetPostersAsync(page, cancellationToken: token);

            var hasPosters = false;
            foreach (var poster in posters)
            {
                using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["AnimeId"] = poster.AnimeId });
                if (!string.IsNullOrEmpty(poster.OriginalUrl))
                {
                    var bytes = await _posterListProvider.LoadImageBytesAsync(poster.OriginalUrl, token);

                    await _posterStore.SavePosterAsync(poster.AnimeId, bytes);

                    if (_options.QueriesInterval > TimeSpan.Zero) await Task.Delay(_options.QueriesInterval, token);
                }

                _logger.LogInformation("Poster has been loaded");
                hasPosters = true;
            }

            _logger.LogInformation("Posters page has been loaded");
            if (!hasPosters) break;
        }
    }
}
