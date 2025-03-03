using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.UI.Configs;

[ExcludeFromCodeCoverage]
public class ManagedIdentityConfig
{
    public const string SectionName = "ManagedIdentity";

    public required string ClientId { get; set; }
}
