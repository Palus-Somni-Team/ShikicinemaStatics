using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace ShikicinemaStatics.Posters;

public class PosterStore : IPosterStore
{
    private readonly string _basePath;

    public PosterStore(IOptionsSnapshot<StaticFileOptions> options)
    {
        _basePath = options.Value.PhysicalPath;
    }

    public async Task SavePosterAsync(string animeId, byte[] poster, string extension)
    {
        var filePath = Path.Combine(_basePath, $"{animeId}.{extension}");
        var exists = File.Exists(filePath);
        if (!exists)
        {
            await File.WriteAllBytesAsync(filePath, poster);
            return;
        }
        
        var existing = await File.ReadAllBytesAsync(filePath);
        var same = ByteArraysEqual(MD5.HashData(existing), MD5.HashData(poster));
        
        if (!same) await File.WriteAllBytesAsync(filePath, poster);
    }

    private static bool ByteArraysEqual(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        return left.SequenceEqual(right);
    }
}