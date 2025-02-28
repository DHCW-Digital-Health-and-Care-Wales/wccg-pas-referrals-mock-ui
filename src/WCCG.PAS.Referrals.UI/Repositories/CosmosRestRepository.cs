using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.UI.Configs;
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
    private const int DefaultMaxItemCount = 10;

    public CosmosRestRepository(ILogger<CosmosRestRepository<T>> logger, IHttpClientFactory httpClientFactory, IOptions<CosmosConfig> cosmosConfig)
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
        var result = new List<T>();
        string? continuationToken = null;

        do
        {
            var (items, token) = await GetAllDocumentsPagedAsync(DefaultMaxItemCount, continuationToken);
            continuationToken = token;
            result.AddRange(items);
        }
        while (!string.IsNullOrEmpty(continuationToken));

        return result;
    }

    private async Task<(IEnumerable<T> Items, string? ContinuationToken)> GetAllDocumentsPagedAsync(int maxItemCount, string? continuationToken = null)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(CosmosConfig.CosmosHttpClientName);
            using var request = new HttpRequestMessage(HttpMethod.Get, _cosmosConfig.ApimGetAllDocumentsEndpoint);
            request.Headers.Add(MaxItemCountHeaderName, $"{maxItemCount}");

            if (!string.IsNullOrWhiteSpace(continuationToken))
            {
                request.Headers.Add(ContinuationTokenHeaderName, continuationToken);
            }

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            response.Headers.TryGetValues(ContinuationTokenHeaderName, out var continuationValues);
            var nextContinuationToken = continuationValues?.FirstOrDefault();

            var content = await response.Content.ReadAsStringAsync();
            var documentsResponse = JsonSerializer.Deserialize<CosmosRestResponse<T>>(content, _jsonSerializerOptions);

            return (documentsResponse!.Documents!, nextContinuationToken);
        }
        catch (Exception ex)
        {
            _logger.LogErrorRetrievingDocuments(ex);
            throw;
        }
    }

    public async Task<T> GetByIdAsync(string id)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(CosmosConfig.CosmosHttpClientName);

            var endpointPath = string.Format(CultureInfo.InvariantCulture, _cosmosConfig.ApimGetDocumentByIdEndpoint, id);
            using var request = new HttpRequestMessage(HttpMethod.Get, endpointPath);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var documentsResponse = JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions);

            return documentsResponse!;
        }
        catch (Exception ex)
        {
            _logger.LogErrorRetrievingDocumentById(ex, id);
            throw;
        }
    }

    public async Task<bool> UpsertAsync(T item)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(CosmosConfig.CosmosHttpClientName);
            using var request = new HttpRequestMessage(HttpMethod.Post, _cosmosConfig.ApimCreateDocumentEndpoint);

            var documentJson = JsonSerializer.Serialize(item, _jsonSerializerOptions);
            request.Content = new StringContent(documentJson, Encoding.UTF8, "application/json");
            request.Headers.Add(UpsertHeaderName, bool.TrueString);

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var documentsResponse = JsonSerializer.Deserialize<JsonElement>(content, _jsonSerializerOptions);
                _logger.LogNewDocumentCreated(documentsResponse.GetProperty("id").GetRawText());

                return true;
            }

            var exception = new HttpRequestException(ParseErrorResponse(content));
            _logger.LogErrorCreatingNewDocumentById(exception);
            throw exception;

        }
        catch (Exception ex)
        {
            _logger.LogErrorCreatingNewDocumentById(ex);
            throw;
        }
    }

    private static string? ParseErrorResponse(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);

            if (document.RootElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString();
            }
            if (document.RootElement.TryGetProperty("error", out var errorElement) && errorElement.TryGetProperty("message", out var errorMessageElement))
            {
                return errorMessageElement.GetString();
            }

            return responseContent;
        }
        catch
        {
            return responseContent;
        }
    }
}
