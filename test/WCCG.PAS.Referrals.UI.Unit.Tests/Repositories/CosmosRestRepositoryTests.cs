using System.Globalization;
using System.Net.Mime;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using WCCG.PAS.Referrals.UI.Configs;
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
        var exception = _fixture.Create<Exception>();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, $"/{_cosmosConfig.ApimGetAllDocumentsEndpoint}")
            .WithHeaders("max-item-count", _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture))
            .WithHeaders("x-ms-continuation", string.Empty)
            .Throw(exception);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var action = _sut.GetAllAsync;

        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage(exception.Message);
    }

    [Fact]
    public async Task GetAllAsyncShouldAddRequiredHeaders()
    {
        //Arrange
        var response = _fixture.Create<CosmosRestResponse<string>>();
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, $"/{_cosmosConfig.ApimGetAllDocumentsEndpoint}")
            .WithHeaders("max-item-count", _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture))
            .WithHeaders("x-ms-continuation", string.Empty)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        await _sut.GetAllAsync();

        //Assert
        mockHttp.VerifyNoOutstandingExpectation();
        mockHttp.VerifyNoOutstandingRequest();
    }

    [Fact]
    public async Task GetAllAsyncShouldProvideNewContinuationTokens()
    {
        //Arrange
        var response = _fixture.Create<CosmosRestResponse<string>>();
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);
        var newContinuationToken = _fixture.Create<string>();

        var mockHttp = new MockHttpMessageHandler();

        mockHttp.Expect(HttpMethod.Get, $"/{_cosmosConfig.ApimGetAllDocumentsEndpoint}")
            .WithHeaders("max-item-count", _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture))
            .WithHeaders("x-ms-continuation", string.Empty)
            .Respond([new KeyValuePair<string, string>("x-ms-continuation", newContinuationToken)], MediaTypeNames.Application.Json,
                responseJson);

        mockHttp.Expect(HttpMethod.Get, $"/{_cosmosConfig.ApimGetAllDocumentsEndpoint}")
            .WithHeaders("max-item-count", _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture))
            .WithHeaders("x-ms-continuation", newContinuationToken)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        await _sut.GetAllAsync();

        //Assert
        mockHttp.VerifyNoOutstandingExpectation();
        mockHttp.VerifyNoOutstandingRequest();
    }

    [Fact]
    public async Task GetAllAsyncShouldReturnDocuments()
    {
        //Arrange
        var response = _fixture.Create<CosmosRestResponse<string>>();
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, $"/{_cosmosConfig.ApimGetAllDocumentsEndpoint}")
            .WithHeaders("max-item-count", _cosmosConfig.MaxItemCountPerQuery.ToString(CultureInfo.InvariantCulture))
            .WithHeaders("x-ms-continuation", string.Empty)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var result = await _sut.GetAllAsync();

        //Assert
        result.Should().BeEquivalentTo(response.Documents);
    }

    [Fact]
    public async Task GetByIdAsyncShouldRethrowException()
    {
        //Arrange
        var id = _fixture.Create<string>();
        var exception = _fixture.Create<Exception>();
        var endpointWithoutId = _cosmosConfig.ApimGetDocumentByIdEndpoint.Split('/').First();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, $"/{endpointWithoutId}/{id}")
            .Throw(exception);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var action = async () => await _sut.GetByIdAsync(id);

        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage(exception.Message);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnElementById()
    {
        //Arrange
        var id = _fixture.Create<string>();
        var response = _fixture.Create<string>();
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);
        var endpointWithoutId = _cosmosConfig.ApimGetDocumentByIdEndpoint.Split('/').First();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, $"/{endpointWithoutId}/{id}")
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var result = await _sut.GetByIdAsync(id);

        //Assert
        result.Should().Be(response);
    }

    [Fact]
    public async Task UpsertAsyncShouldRethrowException()
    {
        //Arrange
        var requestBody = _fixture.Create<string>();
        var requestBodyJson = JsonSerializer.Serialize(requestBody, _jsonSerializerOptions);
        var exception = _fixture.Create<Exception>();


        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, $"/{_cosmosConfig.ApimCreateDocumentEndpoint}")
            .WithHeaders("is-upsert", bool.TrueString)
            .WithContent(requestBodyJson)
            .Throw(exception);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var action = async () => await _sut.UpsertAsync(requestBody);

        //Assert
        await action.Should().ThrowAsync<Exception>().WithMessage(exception.Message);
    }

    [Fact]
    public async Task UpsertAsyncShouldAddRequiredHeader()
    {
        //Arrange
        var requestBody = _fixture.Create<string>();
        var requestBodyJson = JsonSerializer.Serialize(requestBody, _jsonSerializerOptions);
        var response = new { id = _fixture.Create<string>() };
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, $"/{_cosmosConfig.ApimCreateDocumentEndpoint}")
            .WithHeaders("is-upsert", bool.TrueString)
            .WithContent(requestBodyJson)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        await _sut.UpsertAsync(requestBody);

        //Assert
        mockHttp.VerifyNoOutstandingExpectation();
        mockHttp.VerifyNoOutstandingRequest();
    }

    [Fact]
    public async Task UpsertAsyncShouldReturnTrueWhenUpsertCompleted()
    {
        //Arrange
        var requestBody = _fixture.Create<string>();
        var requestBodyJson = JsonSerializer.Serialize(requestBody, _jsonSerializerOptions);

        var upsertId = _fixture.Create<string>();
        var response = new { id = upsertId };
        var responseJson = JsonSerializer.Serialize(response, _jsonSerializerOptions);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Post, $"/{_cosmosConfig.ApimCreateDocumentEndpoint}")
            .WithHeaders("is-upsert", bool.TrueString)
            .WithContent(requestBodyJson)
            .Respond(MediaTypeNames.Application.Json, responseJson);

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://some.com");

        _fixture.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(CosmosConfig.CosmosHttpClientName))
            .Returns(client);

        //Act
        var result = await _sut.UpsertAsync(requestBody);

        //Assert
        result.Should().BeTrue();
    }
}
