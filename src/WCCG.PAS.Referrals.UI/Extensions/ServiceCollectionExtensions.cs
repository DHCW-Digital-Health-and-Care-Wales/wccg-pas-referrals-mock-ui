using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Mime;
using Azure.Identity;
using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.UI.Configs;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Repositories;
using WCCG.PAS.Referrals.UI.Services;
using WCCG.PAS.Referrals.UI.Validators;

namespace WCCG.PAS.Referrals.UI.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddApplicationInsights(this IServiceCollection services, bool isDevelopmentEnvironment, IConfiguration configuration)
    {
        var appInsightsConnectionString = configuration.GetRequiredSection(ApplicationInsightsConfig.SectionName)
            .GetValue<string>(nameof(ApplicationInsightsConfig.ConnectionString));

        services.AddApplicationInsightsTelemetry(options => options.ConnectionString = appInsightsConnectionString);
        services.Configure<TelemetryConfiguration>(config =>
        {
            if (isDevelopmentEnvironment)
            {
                config.SetAzureTokenCredential(new AzureCliCredential());
                return;
            }

            var clientId = configuration.GetRequiredSection(ManagedIdentityConfig.SectionName)
                .GetValue<string>(nameof(ManagedIdentityConfig.ClientId));
            config.SetAzureTokenCredential(new ManagedIdentityCredential(clientId));
        });
    }

    public static void AddCosmosClient(this IServiceCollection services)
    {
        services.AddHttpClient(CosmosConfig.CosmosHttpClientName, (provider, client) =>
        {
            var cosmosConfig = provider.GetRequiredService<IOptions<CosmosConfig>>().Value;

            client.BaseAddress = new Uri(cosmosConfig.ApimEndpoint);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            client.DefaultRequestHeaders.Add(cosmosConfig.ApimSubscriptionHeaderName, cosmosConfig.ApimSubscriptionKey);
            client.Timeout = TimeSpan.FromSeconds(cosmosConfig.TimeoutSeconds);
        });
    }

    public static void AddCosmosRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ICosmosRepository<ReferralDbModel>, CosmosRestRepository<ReferralDbModel>>();
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IReferralService, ReferralService>();
    }

    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<ReferralDbModel>, ReferralValidator>();
    }
}
