using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.UI.Configs;

[ExcludeFromCodeCoverage]
public class ApplicationInsightsConfig
{
    public static string SectionName => "ApplicationInsights";

    public required string ConnectionString { get; set; }
}
