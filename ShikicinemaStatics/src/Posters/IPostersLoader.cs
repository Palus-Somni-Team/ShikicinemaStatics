namespace ShikicinemaStatics.Posters;

internal interface IPostersLoader
{
    Task LoadPostersAsync(CancellationToken token = default);
}
