using System.Text.Json.Serialization;

namespace WCCG.PAS.Referrals.UI.DbModels;

public class ReferralDbModel
{
    [JsonPropertyName("id")]
    public required string? ReferralId { get; set; }

    [JsonPropertyName("caseno")]
    public string? CaseNumber { get; set; }

    [JsonPropertyName("nhs")]
    public string? NhsNumber { get; set; }

    [JsonPropertyName("datRef")]
    public string? CreationDate { get; set; }

    [JsonPropertyName("wlist")]
    public string? WaitingList { get; set; }

    [JsonPropertyName("intentRefer")]
    public string? IntendedManagement { get; set; }

    [JsonPropertyName("gpRef")]
    public string? Referrer { get; set; }

    [JsonPropertyName("gpPrac")]
    public string? ReferrerAddress { get; set; }

    [JsonPropertyName("regGp")]
    public string? PatientGpCode { get; set; }

    [JsonPropertyName("regPrac")]
    public string? PatientGpPracticeCode { get; set; }

    [JsonPropertyName("postcode")]
    public string? PatientPostcode { get; set; }

    [JsonPropertyName("dhaCode")]
    public string? PatientHealthBoardAreaCode { get; set; }

    [JsonPropertyName("sourceRefer")]
    public string? ReferrerSourceType { get; set; }

    [JsonPropertyName("lttrPrty")]
    public string? LetterPriority { get; set; }

    [JsonPropertyName("datonsys")]
    public string? HealthBoardReceiveDate { get; set; }

    [JsonPropertyName("cons")]
    public string? ReferralAssignedConsultant { get; set; }

    [JsonPropertyName("loc")]
    public string? ReferralAssignedLocation { get; set; }

    [JsonPropertyName("category")]
    public string? PatientCategory { get; set; }

    [JsonPropertyName("consPrty")]
    public string? Priority { get; set; }

    [JsonPropertyName("dateBooked")]
    public string? BookingDate { get; set; }

    [JsonPropertyName("trtDate")]
    public string? TreatmentDate { get; set; }

    [JsonPropertyName("spec")]
    public string? SpecialityIdentifier { get; set; }

    [JsonPropertyName("firstApproxFreq")]
    public string? RepeatPeriod { get; set; }

    [JsonPropertyName("firstApproxAppt")]
    public string? FirstAppointmentDate { get; set; }

    [JsonPropertyName("healthRiskFactor")]
    public string? HealthRiskFactor { get; set; }
}
