using System.Text.Json;
using Polly;
using ShikicinemaStatics.Polly;
using ShikicinemaStatics.Posters.Shikimori;

namespace ShikicinemaStatics.Posters.PosterListProviders;

public abstract class PosterListProviderBase : IPosterListProvider
{
    private readonly HttpClient _http;
    private readonly ResiliencePipeline _pipeline;

    protected PosterListProviderBase(IHttpClientFactory httpClientFactory, ResiliencePipeline pipeline)
    {
        _http = httpClientFactory.CreateClient(nameof(PosterListProviderBase));
        _pipeline = pipeline;
    }

    public abstract Task<IEnumerable<Poster>> GetPostersAsync(int page, int pageSize = 50, CancellationToken token = default);

    public async Task<byte[]> LoadImageBytesAsync(string url, CancellationToken token = default)
    {
        using var context = new SharedPollyContext($"{GetType().Name}.{nameof(LoadImageBytesAsync)}", token);

        return await _pipeline.ExecuteAsync(
            async ctx => await _http.GetByteArrayAsync(url, ctx.CancellationToken),
            context.PollyContext
        );
    }

    protected async Task<List<Anime>> QueryAnimesAsync(string query, CancellationToken token)
    {
        using var context = new SharedPollyContext($"{GetType().Name}.{nameof(QueryAnimesAsync)}", token);
        return await _pipeline.ExecuteAsync(
            async ctx =>
            {
                var response = await _http.PostAsJsonAsync("/api/graphql", new GqlRequest { Query = query }, ctx.CancellationToken);
                var responseString = await response.Content.ReadAsStringAsync(token);
                var responseBody = JsonSerializer.Deserialize<GqlResponse<GetAnimesResponse>>(responseString);
                return responseBody?.Data?.Animes ?? throw new Exception("Cannot parse response body: " + responseString);
            },
            context.PollyContext
        );
    }
}
