using System.Security.Cryptography;
using ImageMagick;
using Microsoft.Extensions.Options;

namespace ShikicinemaStatics.Posters;

public class PosterStore : IPosterStore
{
    private readonly string _basePath;
    private readonly ILogger<PosterStore> _logger;

    public PosterStore(IOptionsSnapshot<StaticFileOptions> options, ILogger<PosterStore> logger)
    {
        _basePath = options.Value.PhysicalPath;
        _logger = logger;
    }

    public async Task SavePosterAsync(string animeId, byte[] poster)
    {
        var filePath = Path.Combine(_basePath, $"{animeId}.jpeg");
        var image = new MagickImage(poster);
        var exists = File.Exists(filePath);
        if (!exists)
        {
            await SaveAsync(animeId, image);
            return;
        }

        var existing = await File.ReadAllBytesAsync(filePath);
        var same = ByteArraysEqual(MD5.HashData(existing), MD5.HashData(image.ToByteArray(MagickFormat.Jpeg)));

        if (same)
        {
            _logger.LogInformation("Poster already up-to-date");
            return;
        }

        await SaveAsync(animeId, image);
    }

    public int? GetLastLoadedAnimeId()
    {
        var lastId = Directory.EnumerateFileSystemEntries(_basePath, "*.jpeg")
            .Select(path =>
            {
                var lastId = Path.GetFileNameWithoutExtension(path);
                return int.TryParse(lastId, out var id) ? id : 0;
            })
            .Max();

        return lastId;
    }

    private static bool ByteArraysEqual(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        return left.SequenceEqual(right);
    }

    private async Task SaveAsync(string animeId, MagickImage image)
    {
        _logger.LogInformation("Saving webp");
        var webpPath = Path.Combine(_basePath, $"{animeId}.{nameof(MagickFormat.WebP).ToLowerInvariant()}");
        var webp = image.Clone();
        webp.Resize(new Percentage(35));
        await webp.WriteAsync(webpPath, MagickFormat.WebP);
        _logger.LogInformation("Webp saved");

        _logger.LogInformation("Saving jpeg placeholder");
        var placeholderPath = Path.Combine(_basePath, $"{animeId}-placeholder.{nameof(MagickFormat.Jpeg).ToLowerInvariant()}");
        var placeholder = image.Clone();
        placeholder.Resize(new Percentage(20));
        placeholder.Blur(0.3, 1.0);
        placeholder.Quality = 40;
        await placeholder.WriteAsync(placeholderPath, MagickFormat.Jpeg);
        _logger.LogInformation("Jpeg placeholder saved");

        _logger.LogInformation("Saving avif");
        var avifPath = Path.Combine(_basePath, $"{animeId}.{nameof(MagickFormat.Avif).ToLowerInvariant()}");
        await image.WriteAsync(avifPath, MagickFormat.Avif);
        _logger.LogInformation("Avif saved");

        // сохраняем jpeg последним т.к. на него будем смотреть чтобы понять загружена картинка или еще нет
        _logger.LogInformation("Saving jpeg");
        var jpegPath = Path.Combine(_basePath, $"{animeId}.{nameof(MagickFormat.Jpeg).ToLowerInvariant()}");
        await image.WriteAsync(jpegPath, MagickFormat.Jpeg);
        _logger.LogInformation("Jpeg saved");
    }
}
