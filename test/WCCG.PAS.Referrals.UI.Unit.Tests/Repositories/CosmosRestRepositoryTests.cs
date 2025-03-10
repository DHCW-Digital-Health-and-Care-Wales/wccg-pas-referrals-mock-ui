using System.Globalization;
using System.Net.Mime;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using WCCG.PAS.Referrals.UI.Configuration;
using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Repositories;
using WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Repositories;

public class CosmosRestRepositoryTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();
    private readonly CosmosRestRepository<string> _sut;

    private readonly CosmosConfig _cosmosConfig;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CosmosRestRepositoryTests()
    {
        _cosmosConfig = _fixture.Build<CosmosConfig>()
            .With(x => x.ApimGetDocumentByIdEndpoint, _fixture.Create<string>() + "/{0}")
            .Create();
        _fixture.Mock<IOptions<CosmosConfig>>().Setup(x => x.Value).Returns(_cosmosConfig);

        _sut = _fixture.CreateWithFrozen<CosmosRestRepository<string>>();
    }

    [Fact]
    public async Task GetAllAsyncShouldRethrowException()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var exception = _fixture.Create<Exception>();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, $"/{_cosmosConfig.ApimGetAllDocumentsEndpoint}")
            .WithHeaders("max-item-count", _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture))
            .WithHeaders("x-ms-continuation", string.Empty)
            .WithHeaders(_cosmosConfig.ApimSubscriptionHeaderName, apimKey)
            .Throw(exception);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var action = async () => await _sut.GetAllAsync(apimKey);

        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage(exception.Message);
    }

    [Fact]
    public async Task GetAllAsyncShouldAddRequiredHeaders()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var response = _fixture.Create<CosmosRestResponse<string>>();
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, $"/{_cosmosConfig.ApimGetAllDocumentsEndpoint}")
            .WithHeaders("max-item-count", _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture))
            .WithHeaders("x-ms-continuation", string.Empty)
            .WithHeaders(_cosmosConfig.ApimSubscriptionHeaderName, apimKey)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        await _sut.GetAllAsync(apimKey);

        //Assert
        mockHttp.VerifyNoOutstandingExpectation();
        mockHttp.VerifyNoOutstandingRequest();
    }

    [Fact]
    public async Task GetAllAsyncShouldProvideNewContinuationTokens()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var response = _fixture.Create<CosmosRestResponse<string>>();
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);
        var newContinuationToken = _fixture.Create<string>();

        var mockHttp = new MockHttpMessageHandler();

        mockHttp.Expect(HttpMethod.Get, $"/{_cosmosConfig.ApimGetAllDocumentsEndpoint}")
            .WithHeaders("max-item-count", _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture))
            .WithHeaders("x-ms-continuation", string.Empty)
            .WithHeaders(_cosmosConfig.ApimSubscriptionHeaderName, apimKey)
            .Respond([new KeyValuePair<string, string>("x-ms-continuation", newContinuationToken)], MediaTypeNames.Application.Json,
                responseJson);

        mockHttp.Expect(HttpMethod.Get, $"/{_cosmosConfig.ApimGetAllDocumentsEndpoint}")
            .WithHeaders("max-item-count", _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture))
            .WithHeaders("x-ms-continuation", newContinuationToken)
            .WithHeaders(_cosmosConfig.ApimSubscriptionHeaderName, apimKey)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        await _sut.GetAllAsync(apimKey);

        //Assert
        mockHttp.VerifyNoOutstandingExpectation();
        mockHttp.VerifyNoOutstandingRequest();
    }

    [Fact]
    public async Task GetAllAsyncShouldReturnDocuments()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var response = _fixture.Create<CosmosRestResponse<string>>();
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, $"/{_cosmosConfig.ApimGetAllDocumentsEndpoint}")
            .WithHeaders("max-item-count", _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture))
            .WithHeaders("x-ms-continuation", string.Empty)
            .WithHeaders(_cosmosConfig.ApimSubscriptionHeaderName, apimKey)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var result = await _sut.GetAllAsync(apimKey);

        //Assert
        result.Should().BeEquivalentTo(response.Documents);
    }

    [Fact]
    public async Task GetByIdAsyncShouldRethrowException()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var id = _fixture.Create<string>();
        var exception = _fixture.Create<Exception>();
        var endpointWithoutId = _cosmosConfig.ApimGetDocumentByIdEndpoint.Split('/').First();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, $"/{endpointWithoutId}/{id}")
            .WithHeaders(_cosmosConfig.ApimSubscriptionHeaderName, apimKey)
            .Throw(exception);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var action = async () => await _sut.GetByIdAsync(apimKey, id);

        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage(exception.Message);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnElementById()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var id = _fixture.Create<string>();
        var response = _fixture.Create<string>();
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);
        var endpointWithoutId = _cosmosConfig.ApimGetDocumentByIdEndpoint.Split('/').First();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, $"/{endpointWithoutId}/{id}")
            .WithHeaders(_cosmosConfig.ApimSubscriptionHeaderName, apimKey)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var result = await _sut.GetByIdAsync(apimKey, id);

        //Assert
        result.Should().Be(response);
    }

    [Fact]
    public async Task UpsertAsyncShouldRethrowException()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var requestBody = _fixture.Create<string>();
        var requestBodyJson = JsonSerializer.Serialize(requestBody, _jsonSerializerOptions);
        var exception = _fixture.Create<Exception>();


        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, $"/{_cosmosConfig.ApimCreateDocumentEndpoint}")
            .WithHeaders("is-upsert", bool.TrueString)
            .WithHeaders(_cosmosConfig.ApimSubscriptionHeaderName, apimKey)
            .WithContent(requestBodyJson)
            .Throw(exception);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var action = async () => await _sut.UpsertAsync(apimKey, requestBody);

        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage(exception.Message);
    }

    [Fact]
    public async Task UpsertAsyncShouldAddRequiredHeader()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var requestBody = _fixture.Create<string>();
        var requestBodyJson = JsonSerializer.Serialize(requestBody, _jsonSerializerOptions);
        var response = new { id = _fixture.Create<string>() };
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, $"/{_cosmosConfig.ApimCreateDocumentEndpoint}")
            .WithHeaders("is-upsert", bool.TrueString)
            .WithHeaders(_cosmosConfig.ApimSubscriptionHeaderName, apimKey)
            .WithContent(requestBodyJson)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        await _sut.UpsertAsync(apimKey, requestBody);

        //Assert
        mockHttp.VerifyNoOutstandingExpectation();
        mockHttp.VerifyNoOutstandingRequest();
    }

    [Fact]
    public async Task UpsertAsyncShouldReturnTrueWhenUpsertCompleted()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var requestBody = _fixture.Create<string>();
        var requestBodyJson = JsonSerializer.Serialize(requestBody, _jsonSerializerOptions);

        var upsertId = _fixture.Create<string>();
        var response = new { id = upsertId };
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, $"/{_cosmosConfig.ApimCreateDocumentEndpoint}")
            .WithHeaders("is-upsert", bool.TrueString)
            .WithHeaders(_cosmosConfig.ApimSubscriptionHeaderName, apimKey)
            .WithContent(requestBodyJson)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var result = await _sut.UpsertAsync(apimKey, requestBody);

        //Assert
        result.Should().BeTrue();
    }
}
