using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.UI.Configuration;
using WCCG.PAS.Referrals.UI.Extensions;
using WCCG.PAS.Referrals.UI.Models;

namespace WCCG.PAS.Referrals.UI.Repositories;

public class CosmosRestRepository<T> : ICosmosRepository<T>
{
    private readonly ILogger<CosmosRestRepository<T>> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CosmosConfig _cosmosConfig;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private const string MaxItemCountHeaderName = "max-item-count";
    private const string ContinuationTokenHeaderName = "x-ms-continuation";
    private const string UpsertHeaderName = "is-upsert";

    private const string DetailsKey = "Details";
    private const string NotApplicable = "N/A";

    public CosmosRestRepository(ILogger<CosmosRestRepository<T>> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<CosmosConfig> cosmosConfig)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _cosmosConfig = cosmosConfig.Value;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        using var client = _httpClientFactory.CreateClient(CosmosConfig.CosmosHttpClientName);

        var maxItemCount = _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture);
        client.DefaultRequestHeaders.Add(MaxItemCountHeaderName, maxItemCount);

        var result = new List<T>();
        string? continuationToken = null;

        do
        {
            var (items, nextContinuationToken) = await GetAllDocumentsPagedAsync(client, continuationToken);
            continuationToken = nextContinuationToken;
            result.AddRange(items);
        } while (!string.IsNullOrEmpty(continuationToken));

        return result;
    }

    private async Task<(IEnumerable<T> Items, string? ContinuationToken)> GetAllDocumentsPagedAsync(HttpClient client,
        string? continuationToken)
    {
        try
        {
            client.DefaultRequestHeaders.Remove(ContinuationTokenHeaderName);
            client.DefaultRequestHeaders.Add(ContinuationTokenHeaderName, continuationToken ?? string.Empty);

            var response = await client.GetAsync(_cosmosConfig.ApimGetAllDocumentsEndpoint);
            await response.EnsureSuccessStatusCodeWithDataAsync(response.Content);

            response.Headers.TryGetValues(ContinuationTokenHeaderName, out var continuationValues);
            var nextContinuationToken = continuationValues?.FirstOrDefault();

            var content = await response.Content.ReadAsStringAsync();
            var documentsResponse = JsonSerializer.Deserialize<CosmosRestResponse<T>>(content, _jsonSerializerOptions);

            return (documentsResponse!.Documents!, nextContinuationToken);
        }
        catch (Exception ex)
        {
            var errorDetails = ex.Data[DetailsKey]?.ToString();

            _logger.LogErrorRetrievingDocuments(ex, errorDetails ?? NotApplicable);
            throw;
        }
    }

    public async Task<T> GetByIdAsync(string id)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(CosmosConfig.CosmosHttpClientName);

            var endpointPath = string.Format(CultureInfo.InvariantCulture, _cosmosConfig.ApimGetDocumentByIdEndpoint, id);

            var response = await client.GetAsync(endpointPath);
            await response.EnsureSuccessStatusCodeWithDataAsync(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions)!;
        }
        catch (Exception ex)
        {
            var errorDetails = ex.Data[DetailsKey]?.ToString();

            _logger.LogErrorRetrievingDocumentById(ex, id, errorDetails ?? NotApplicable);
            throw;
        }
    }

    public async Task<bool> UpsertAsync(T item)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(CosmosConfig.CosmosHttpClientName);
            client.DefaultRequestHeaders.Add(UpsertHeaderName, bool.TrueString);

            var response = await client.PostAsJsonAsync(_cosmosConfig.ApimCreateDocumentEndpoint, item, _jsonSerializerOptions);
            await response.EnsureSuccessStatusCodeWithDataAsync(response.Content);

            var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonSerializerOptions);

            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    _logger.LogNewDocumentCreated(content.GetProperty("id").GetRawText());
                    break;
                case HttpStatusCode.OK:
                    _logger.LogDocumentUpdated(content.GetProperty("id").GetRawText());
                    break;
                default:
                    throw new NotSupportedException();
            }

            return true;
        }
        catch (Exception ex)
        {
            var errorDetails = ex.Data[DetailsKey]?.ToString();

            _logger.LogErrorCreatingOrUpdatingDocument(ex, errorDetails ?? NotApplicable);
            throw;
        }
    }
}
