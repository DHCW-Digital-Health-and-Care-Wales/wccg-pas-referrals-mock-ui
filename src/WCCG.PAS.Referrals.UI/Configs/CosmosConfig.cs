namespace WCCG.PAS.Referrals.UI.Configs;

public class CosmosConfig
{
    public const string CosmosHttpClientName = "CosmosRestClient";

    public required string ApimEndpoint { get; init; }
    public required string ApimSubscriptionHeaderName { get; init; }
    public required string ApimSubscriptionKey { get; init; }
    public required string ApimGetAllDocumentsEndpoint { get; init; }
    public required string ApimGetDocumentByIdEndpoint { get; init; }
    public required string ApimCreateDocumentEndpoint { get; init; }
    public required int TimeoutSeconds { get; init; }
}
