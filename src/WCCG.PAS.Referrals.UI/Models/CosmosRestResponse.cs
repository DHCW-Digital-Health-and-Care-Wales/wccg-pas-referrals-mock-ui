using System.Text.Json.Serialization;

namespace WCCG.PAS.Referrals.UI.Models;

public class CosmosRestResponse<T>
{
    [JsonPropertyName("_rid")]
    public string? Rid { get; init; }

    [JsonPropertyName("Documents")]
    public List<T>? Documents { get; init; }

    [JsonPropertyName("_count")]
    public int? Count { get; init; }
}
