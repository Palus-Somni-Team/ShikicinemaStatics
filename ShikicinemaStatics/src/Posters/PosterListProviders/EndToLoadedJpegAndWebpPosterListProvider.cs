using ShikicinemaStatics.Posters.Shikimori;

namespace ShikicinemaStatics.Posters.PosterListProviders;

public class EndToLoadedJpegAndWebpPosterListProvider : PosterListProviderBase
{
    private readonly Lazy<int> _lastLoadedAnimeId;

    public EndToLoadedJpegAndWebpPosterListProvider(IHttpClientFactory httpClientFactory, IPosterStore store)
        : base(httpClientFactory)
    {
        _lastLoadedAnimeId = new Lazy<int>(() =>
        {
            var jpegId = store.GetLastLoadedAnimeId("jpeg");
            var webpId = store.GetLastLoadedAnimeId("webp");

            return Math.Min(jpegId ?? 0, webpId ?? 0);
        });
    }

    public override async Task<IEnumerable<Poster>> GetPostersAsync(int page, int pageSize = 50, CancellationToken token = default)
    {
        const string gqlQuery =
            """
            {{
              animes(page: {0}, limit: {1}, order: id_desc, censored: false) {{
                id
                poster {{ originalUrl mainUrl }}
              }}
            }}
            """;
        var query = string.Format(gqlQuery, page, pageSize);
        var animes = await QueryAnimesAsync(query, token);

        return SelectPosters(animes);
    }

    private IEnumerable<Poster> SelectPosters(List<Anime> animes)
    {
        foreach (var anime in animes)
        {
            if (anime.Poster == null)
            {
                continue;
            }

            if (!int.TryParse(anime.Id, out var animeId))
            {
                continue;
            }

            if (animeId <= _lastLoadedAnimeId.Value)
            {
                yield break;
            }

            yield return new Poster(anime.Id, anime.Poster.OriginalUrl, anime.Poster.MainUrl);
        }
    }
}
