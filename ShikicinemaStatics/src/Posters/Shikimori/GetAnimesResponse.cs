using System.Text.Json.Serialization;

namespace ShikicinemaStatics.Posters.Shikimori;

public class GetAnimesResponse
{
    [JsonPropertyName("animes")]
    public List<Anime>? Animes { get; set; }
}
