namespace ShikicinemaStatics.Posters;

public interface IPosterStore
{
    Task SavePosterAsync(string animeId, byte[] poster, string extension);
    int? GetLastLoadedAnimeId(string extension);
}
