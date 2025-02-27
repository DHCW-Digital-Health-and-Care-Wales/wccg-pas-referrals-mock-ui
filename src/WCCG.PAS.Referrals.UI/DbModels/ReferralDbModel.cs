using System.Text.Json.Serialization;

namespace WCCG.PAS.Referrals.UI.DbModels;

public class ReferralDbModel
{
    public string? Id { get; set; }

    [JsonPropertyName("caseno")]
    public string? CaseNumber { get; set; }
}
