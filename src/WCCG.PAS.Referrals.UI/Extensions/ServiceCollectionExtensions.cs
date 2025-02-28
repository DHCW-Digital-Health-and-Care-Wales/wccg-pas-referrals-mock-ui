using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
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
    public static void AddApplicationInsights(this IServiceCollection services, IHostEnvironment environment, string clientId)
    {
        services.AddApplicationInsightsTelemetry();

        services.Configure<TelemetryConfiguration>(config =>
        {
            if (environment.IsDevelopment())
            {
                config.SetAzureTokenCredential(new AzureCliCredential());
                return;
            }

            config.SetAzureTokenCredential(new ManagedIdentityCredential(clientId));
        });
    }

    public static void AddCosmosClient(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddHttpClient(CosmosConfig.CosmosHttpClientName, (provider, client) =>
        {
            var cosmosConfig = provider.GetRequiredService<IOptions<CosmosConfig>>().Value;

            client.BaseAddress = new Uri(cosmosConfig.ApimEndpoint);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
