using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Domain.Account;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.Gateway
{
    [Collection("LogCall collection")]
    public class AccountApiGatewayTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly AccountApiGateway _sut;
        private IConfiguration _configuration;
        private readonly static JsonSerializerOptions _jsonOptions = JsonOptions.CreateJsonOptions();

        private static readonly Guid _correlationId = Guid.NewGuid();
        private const string AccountApiRoute = "https://some-domain.com/api/";
        private const string AccountApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        public AccountApiGatewayTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                                  .Returns(_httpClient);

            var inMemorySettings = new Dictionary<string, string> {
                { "AccountApiUrl", AccountApiRoute },
                { "AccountApiToken", AccountApiToken }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _sut = new AccountApiGateway(_mockHttpClientFactory.Object, _configuration);
        }

        private static string Route(Guid id) => $"{AccountApiRoute}accounts/{id}";

        private static bool ValidateRequest(string expectedRoute, HttpRequestMessage request)
        {
            var correlationIdHeader = request.Headers.GetValues("x-correlation-id")?.FirstOrDefault();
            var apiToken = request.Headers.GetValues("Authorization")?.FirstOrDefault();
            return (request.RequestUri.ToString() == expectedRoute)
                && (apiToken == AccountApiToken)
                && (correlationIdHeader == _correlationId.ToString());
        }

        private void SetupHttpClientResponse(string route, Account response)
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
                { "AccountApiUrl", invalidValue }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action act = () => _ = new AccountApiGateway(_mockHttpClientFactory.Object, _configuration);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ConstructorTestInvalidTokenConfigThrows(string invalidValue)
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "AccountApiUrl", AccountApiRoute },
                { "AccountApiKey", invalidValue }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action act = () => _ = new AccountApiGateway(_mockHttpClientFactory.Object, _configuration);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetAccountByIdAsyncGetExceptionThrown()
        {
            var id = Guid.NewGuid();
            var exMessage = "This is an exception";
            SetupHttpClientException(Route(id), new Exception(exMessage));

            Func<Task<Account>> func =
                async () => await _sut.GetAccountByIdAsync(id, _correlationId).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public void GetAccountByIdAsyncCallFailedExceptionThrown()
        {
            var id = Guid.NewGuid();
            var error = "This is an error message";
            SetupHttpClientErrorResponse(Route(id), error);

            Func<Task<Account>> func =
                async () => await _sut.GetAccountByIdAsync(id, _correlationId).ConfigureAwait(false);

            func.Should().ThrowAsync<GetAccountException>()
                         .WithMessage($"Failed to get account details for id {id}. " +
                         $"Status code: {HttpStatusCode.InternalServerError}; Message: {error}");
        }

        [Fact]
        public async Task GetAccountByIdAsyncNotFoundReturnsNull()
        {
            var id = Guid.NewGuid();
            SetupHttpClientResponse(Route(id), null);

            var result = await _sut.GetAccountByIdAsync(id, _correlationId).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAccountByIdAsyncCallReturnsAccount()
        {
            var id = Guid.NewGuid();
            var account = new Fixture().Create<Account>();
            SetupHttpClientResponse(Route(id), account);

            var result = await _sut.GetAccountByIdAsync(id, _correlationId).ConfigureAwait(false);

            result.Should().BeEquivalentTo(account);
        }
    }
}
