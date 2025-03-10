using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.UI.Configuration;

[ExcludeFromCodeCoverage]
public class CosmosConfig
{
    public const string SectionName = "Cosmos";
    public const string CosmosHttpClientName = "CosmosRestClient";

    [Required]
    public required string ApimEndpoint { get; init; }

    [Required]
    public required string ApimSubscriptionHeaderName { get; init; }

    [Required]
    public required string ApimGetAllDocumentsEndpoint { get; init; }

    [Required]
    public required string ApimGetDocumentByIdEndpoint { get; init; }

    [Required]
    public required string ApimCreateDocumentEndpoint { get; init; }

    [Required]
    public required int TimeoutSeconds { get; init; }

    [Required]
    public required int MaxItemCountPerQuery { get; init; }
}
