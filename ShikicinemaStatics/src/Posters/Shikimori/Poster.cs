using System.Text.Json.Serialization;

namespace ShikicinemaStatics.Posters.Shikimori;

public class Poster
{
    [JsonPropertyName("originalUrl")]
    public string OriginalUrl { get; set; } = null!;

    [JsonPropertyName("mainUrl")]
    public string MainUrl { get; set; } = null!;
}