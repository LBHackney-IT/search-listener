using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.Gateway
{
    [Collection("LogCall collection")]
    public class TenureApiGatewayTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly TenureApiGateway _sut;
        private IConfiguration _configuration;
        private readonly static JsonSerializerOptions _jsonOptions = JsonOptions.CreateJsonOptions();

        private const string TenureApiRoute = "https://some-domain.com/api/";
        private const string TenureApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        public TenureApiGatewayTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                                  .Returns(_httpClient);

            var inMemorySettings = new Dictionary<string, string> {
                { "TenureApiUrl", TenureApiRoute },
                { "TenureApiToken", TenureApiToken }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _sut = new TenureApiGateway(_mockHttpClientFactory.Object, _configuration);
        }

        private static string Route(Guid id) => $"{TenureApiRoute}tenures/{id}";

        private static bool ValidateRequest(string expectedRoute, HttpRequestMessage request)
        {
            return (request.RequestUri.ToString() == expectedRoute)
                && (request.Headers.Authorization.ToString() == TenureApiToken);
        }

        private void SetupHttpClientResponse(string route, TenureInformation response)
        {
            HttpStatusCode statusCode = (response is null) ?
                HttpStatusCode.NotFound : HttpStatusCode.OK;
            HttpContent content = (response is null) ?
                null : new StringContent(JsonSerializer.Serialize(response, _jsonOptions));
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => ValidateRequest(route, y)),
                        ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = statusCode,
                       Content = content,
                   });
        }

        private void SetupHttpClientErrorResponse(string route, string response)
        {
            HttpContent content = (response is null) ? null : new StringContent(response);
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => y.RequestUri.ToString() == route),
                        ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.InternalServerError,
                       Content = content,
                   });
        }

        private void SetupHttpClientException(string route, Exception ex)
        {
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => y.RequestUri.ToString() == route),
                        ItExpr.IsAny<CancellationToken>())
                   .ThrowsAsync(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("sdrtgdfstg")]
        public void ConstructorTestInvalidRouteConfigThrows(string invalidValue)
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "TenureApiUrl", invalidValue }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action act = () => _ = new TenureApiGateway(_mockHttpClientFactory.Object, _configuration);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ConstructorTestInvalidTokenConfigThrows(string invalidValue)
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "TenureApiUrl", TenureApiRoute },
                { "TenureApiToken", invalidValue }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action act = () => _ = new TenureApiGateway(_mockHttpClientFactory.Object, _configuration);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetTenureByIdAsyncGetExceptionThrown()
        {
            var id = Guid.NewGuid();
            var exMessage = "This is an exception";
            SetupHttpClientException(Route(id), new Exception(exMessage));

            Func<Task<TenureInformation>> func =
                async () => await _sut.GetTenureByIdAsync(id).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public void GetTenureByIdAsyncCallFailedExceptionThrown()
        {
            var id = Guid.NewGuid();
            var error = "This is an error message";
            SetupHttpClientErrorResponse(Route(id), error);

            Func<Task<TenureInformation>> func =
                async () => await _sut.GetTenureByIdAsync(id).ConfigureAwait(false);

            func.Should().ThrowAsync<GetTenureException>()
                         .WithMessage($"Failed to get tenure details for id {id}. " +
                         $"Status code: {HttpStatusCode.InternalServerError}; Message: {error}");
        }

        [Fact]
        public async Task GetTenureByIdAsyncNotFoundReturnsNull()
        {
            var id = Guid.NewGuid();
            SetupHttpClientResponse(Route(id), null);

            var result = await _sut.GetTenureByIdAsync(id).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTenureByIdAsyncCallReturnsPerson()
        {
            var id = Guid.NewGuid();
            var tenure = new Fixture().Create<TenureInformation>();
            SetupHttpClientResponse(Route(id), tenure);

            var result = await _sut.GetTenureByIdAsync(id).ConfigureAwait(false);

            result.Should().BeEquivalentTo(tenure);
        }
    }
}
