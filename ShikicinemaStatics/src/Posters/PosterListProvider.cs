using System.Text.Json;
using ShikicinemaStatics.Posters.Shikimori;

namespace ShikicinemaStatics.Posters;

public class PosterListProvider : IPosterListProvider
{
    private readonly HttpClient _http;

    public PosterListProvider(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient(nameof(PosterListProvider));
    }

    public async Task<IEnumerable<Poster>> GetPostersAsync(
        int page,
        int pageSize = 50,
        CancellationToken token = default)
    {
        const string gqlReqTemplate =
            """
            {{
              animes(page: {0}, limit: {1}, order: id, censored: false) {{
                id
                poster {{ originalUrl mainUrl }}
              }}
            }}
            """;

        var query = string.Format(gqlReqTemplate, page, pageSize);
        var response = await _http.PostAsJsonAsync("/api/graphql", new GqlRequest {Query = query}, token);

        var responseString = await response.Content.ReadAsStringAsync(token);
        var responseBody = JsonSerializer.Deserialize<GqlResponse<GetAnimesResponse>>(responseString);
        if (responseBody?.Data?.Animes == null)
        {
            throw new Exception("Cannot parse response body: " + responseString);
        }

        return responseBody.Data.Animes
            .Where(a => a.Poster != null)
            .Select(a => new Poster(a.Id, a.Poster!.OriginalUrl, a.Poster!.MainUrl));
    }
}
