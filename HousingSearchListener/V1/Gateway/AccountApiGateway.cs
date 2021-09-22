using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.Account;
using HousingSearchListener.V1.Infrastructure;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class AccountApiGateway : IAccountApiGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _accountApiRoute;
        private readonly string _accountApiToken;

        private const string ApiTokenHeaderName = "x-api-key";
        private const string AccountApiUrl = "AccountApiUrl";
        private const string AccountApiKey = "AccountApiKey";
        private readonly static JsonSerializerOptions _jsonOptions = JsonOptions.CreateJsonOptions();

        public AccountApiGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _accountApiRoute = configuration.GetValue<string>(AccountApiUrl)?.TrimEnd('/');

            if (string.IsNullOrEmpty(_accountApiRoute) || !Uri.IsWellFormedUriString(_accountApiRoute, UriKind.Absolute))
            {
                throw new ArgumentException($"Configuration does not contain a setting value for the key {AccountApiUrl}.");
            }

            _accountApiToken = configuration.GetValue<string>(AccountApiKey);

            if (string.IsNullOrEmpty(_accountApiToken))
            {
                throw new ArgumentException($"Configuration does not contain a setting value for the key {AccountApiKey}.");
            }
        }

        [LogCall]
        public async Task<Account> GetAccountByIdAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            var getAccountRoute = $"{_accountApiRoute}/accounts/{id}";

            client.DefaultRequestHeaders.Add(ApiTokenHeaderName, _accountApiToken);

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
