using System.Net;
using Moq;
using Moq.Protected;

namespace Jufottaja.Tests;

[TestFixture]
public class ApiClientTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private ApiClient _apiClient;

    [SetUp]
    public void SetUp()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _apiClient = new ApiClient(httpClient);
    }

    [Test]
    public async Task GetJufoChannelId_ReturnsJufoId_WhenValidResponse()
    {
        // Arrange
        var jufoApiQueryParameters = new JufoApiQueryParameters
        {
            Name = "Valid Name"
        };
        const string responseContent = "[{\"Jufo_ID\": \"12345\"}]";

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _apiClient.GetJufoChannelId(jufoApiQueryParameters);

        // Assert
        Assert.That(result, Is.EqualTo("12345"));
    }

    [Test]
    public async Task GetJufoChannelId_ReturnsNoResult_WhenResponseIsEmptyArray()
    {
        // Arrange
        var jufoApiQueryParameters = new JufoApiQueryParameters
        {
            Name = "Valid-Name"
        };
        const string responseContent = "";

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _apiClient.GetJufoChannelId(jufoApiQueryParameters);

        // Assert
        Assert.That(result, Is.EqualTo("NO RESULT"));
    }
    
    
    [Test]
    public async Task GetJufoChannelId_ReturnsMultipleResults_WhenResponseHasMultipleResults()
    {
        // Arrange
        var jufoApiQueryParameters = new JufoApiQueryParameters
        {
            Name = "ValidName"
        };
        const string responseContent = "[{\"Jufo_ID\": \"12345\"}, {\"Jufo_ID\": \"54321\"}]";
        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _apiClient.GetJufoChannelId(jufoApiQueryParameters);

        // Assert
        Assert.That(result, Is.EqualTo("MULTIPLE RESULTS"));
    }

    [Test]
    public async Task GetJufoChannelId_ReturnsEmptyString_WhenInvalidName()
    {
        // Arrange
        var jufoApiQueryParameters = new JufoApiQueryParameters
        {
            Name = "invalid@name"
        };

        // Act
        var result = await _apiClient.GetJufoChannelId(jufoApiQueryParameters);

        // Assert
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public async Task GetJufoChannelId_ThrowsException_OnHttpError()
    {
        // Arrange
        var jufoApiQueryParameters = new JufoApiQueryParameters
        {
            Name = "ValidName"
        };
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Server error");

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _apiClient.GetJufoChannelId(jufoApiQueryParameters));
        Assert.That(ex?.Message, Is.EqualTo("Error while calling the JUFO API."));
    }

    [Test]
    public async Task GetJufoChannelLevel_ReturnsLevel_WhenValidResponse()
    {
        // Arrange
        const string channelId = "12345";
        const string responseContent = "[{\"Level\": \"3\"}]";

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _apiClient.GetJufoChannelLevel(channelId);

        // Assert
        Assert.That(result, Is.EqualTo("3"));
    }

    [Test]
    public async Task GetJufoChannelLevel_ReturnsEmptyString_WhenResponseIsEmptyArray()
    {
        // Arrange
        const string channelId = "12345";
        const string responseContent = "[]";

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        // Act
        var result = await _apiClient.GetJufoChannelLevel(channelId);

        // Assert
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public async Task GetJufoChannelLevel_ThrowsException_OnHttpError()
    {
        // Arrange
        const string channelId = "12345";
        SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request");

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _apiClient.GetJufoChannelLevel(channelId));
        Assert.That(ex?.Message, Is.EqualTo("Error while calling the JUFO API."));
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}