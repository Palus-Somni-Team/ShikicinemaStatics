namespace ShikicinemaStatics.Posters;

public interface IPosterListProvider
{
    public Task<Poster[]> GetPostersAsync(int page, int pageSize = 50, CancellationToken cancellationToken = default);
}
