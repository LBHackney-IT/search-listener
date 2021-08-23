using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.Tenure;
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
    public class TenureApiGateway : ITenureApiGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _getTenureApiRoute;
        private readonly string _getTenureApiToken;

        private const string TenureApiUrl = "TenureApiUrl";
        private const string TenureApiToken = "TenureApiToken";
        private readonly static JsonSerializerOptions _jsonOptions = JsonOptions.CreateJsonOptions();

        public TenureApiGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _getTenureApiRoute = configuration.GetValue<string>(TenureApiUrl)?.TrimEnd('/');
            if (string.IsNullOrEmpty(_getTenureApiRoute) || !Uri.IsWellFormedUriString(_getTenureApiRoute, UriKind.Absolute))
                throw new ArgumentException($"Configuration does not contain a setting value for the key {TenureApiUrl}.");

            _getTenureApiToken = configuration.GetValue<string>(TenureApiToken);
            if (string.IsNullOrEmpty(_getTenureApiToken))
                throw new ArgumentException($"Configuration does not contain a setting value for the key {TenureApiToken}.");
        }

        [LogCall]
        public async Task<TenureInformation> GetTenureByIdAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            var getTenureRoute = $"{_getTenureApiRoute}/tenures/{id}";

            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(_getTenureApiToken);
            var response = await client.GetAsync(new Uri(getTenureRoute))
                                       .ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.NotFound)
                return null;

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonSerializer.Deserialize<TenureInformation>(responseBody, _jsonOptions);

            throw new GetTenureException(id, response.StatusCode, responseBody);
        }
    }
}
