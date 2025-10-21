namespace ShikicinemaStatics.Posters.PosterListProviders;

public interface IPosterListProvider
{
    public Task<IEnumerable<Poster>> GetPostersAsync(int page, int pageSize = 50, CancellationToken cancellationToken = default);
    public Task<byte[]> LoadImageBytesAsync(string url, CancellationToken token = default);
}
