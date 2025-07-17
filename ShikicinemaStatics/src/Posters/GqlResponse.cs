using System.Text.Json.Serialization;

namespace ShikicinemaStatics.Posters;

public class GqlResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}