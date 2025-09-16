using ShikicinemaStatics.Posters.Shikimori;

namespace ShikicinemaStatics.Posters.PosterListProviders;

public class StartToEndPosterListProvider : PosterListProviderBase
{
    public StartToEndPosterListProvider(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
    }

    public override async Task<IEnumerable<Poster>> GetPostersAsync(int page, int pageSize = 50, CancellationToken token = default)
    {
        const string gqlQuery =
            """
            {{
              animes(page: {0}, limit: {1}, order: id, censored: false) {{
                id
                poster {{ originalUrl }}
              }}
            }}
            """;
        var query = string.Format(gqlQuery, page, pageSize);
        var animes = await QueryAnimesAsync(query, token);

        return SelectPosters(animes);
    }

    private static IEnumerable<Poster> SelectPosters(List<Anime> animes)
    {
        return animes
            .Where(p => p.Poster != null)
            .Select(p => new Poster(p.Id, p.Poster!.OriginalUrl));
    }
}
