using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.Account;
using HousingSearchListener.V1.Infrastructure;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class AccountApiGateway : IAccountApiGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _getAccountApiRoute;
        private readonly string _getAccountApiToken;

        private const string AccountApiUrl = "AccountApiUrl";
        private const string AccountApiToken = "AccountApiToken";
        private readonly static JsonSerializerOptions _jsonOptions = JsonOptions.CreateJsonOptions();

        public AccountApiGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _getAccountApiRoute = configuration.GetValue<string>(AccountApiUrl)?.TrimEnd('/');

            if (string.IsNullOrEmpty(_getAccountApiRoute) || !Uri.IsWellFormedUriString(_getAccountApiRoute, UriKind.Absolute))
            {
                throw new ArgumentException($"Configuration does not contain a setting value for the key {AccountApiUrl}.");
            }

            _getAccountApiToken = configuration.GetValue<string>(AccountApiToken);
            
            if (string.IsNullOrEmpty(_getAccountApiToken))
            {
                throw new ArgumentException($"Configuration does not contain a setting value for the key {AccountApiToken}.");
            }
        }

        [LogCall]
        public async Task<Account> GetAccountByIdAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            var getAccountRoute = $"{_getAccountApiRoute}/residents/{id}";
            
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(_getAccountApiToken);

            var response = await RetryService.DoAsync(client.GetAsync(new Uri(getAccountRoute)), maxAttemptCount: 5, delay: TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.NotFound)
            {
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<Account>(responseBody, _jsonOptions);
            }

            throw new GetAccountException(id, response.StatusCode, responseBody);
        }
    }
}
