using System.Text.Json;
using ShikicinemaStatics.Posters.Shikimori;

namespace ShikicinemaStatics.Posters.PosterListProviders;

public abstract class PosterListProviderBase : IPosterListProvider
{
    private readonly HttpClient _http;

    protected PosterListProviderBase(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient(nameof(PosterListProviderBase));
    }

    public abstract Task<IEnumerable<Poster>> GetPostersAsync(int page, int pageSize = 50, CancellationToken token = default);

    protected async Task<List<Anime>> QueryAnimesAsync(string query, CancellationToken token)
    {
        var response = await _http.PostAsJsonAsync("/api/graphql", new GqlRequest {Query = query}, token);

        var responseString = await response.Content.ReadAsStringAsync(token);
        var responseBody = JsonSerializer.Deserialize<GqlResponse<GetAnimesResponse>>(responseString);
        if (responseBody?.Data?.Animes == null)
        {
            throw new Exception("Cannot parse response body: " + responseString);
        }

        return responseBody.Data.Animes;
    }
}
