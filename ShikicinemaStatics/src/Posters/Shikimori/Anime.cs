using System.Text.Json.Serialization;

namespace ShikicinemaStatics.Posters.Shikimori;

public class Anime
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("poster")]
    public Poster? Poster { get; set; }
}
