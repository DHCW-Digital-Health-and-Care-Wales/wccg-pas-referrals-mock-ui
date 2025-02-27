namespace WCCG.PAS.Referrals.UI.Configs;

public class CosmosConfig
{
    public const string CosmosHttpClientName = "CosmosRestClient";

    public required string ApimEndpoint { get; set; }
    public required string ApimSubscriptionHeaderName { get; set; }
    public required string ApimSubscriptionKey { get; set; }
    public required string ApimGetAllEndpoint { get; set; }
    public required string ApimGetByIdEndpoint { get; set; }
    public required int TimeoutSeconds { get; set; }
}
