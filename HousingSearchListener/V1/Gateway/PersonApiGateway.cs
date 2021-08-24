using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.Person;
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
    public class PersonApiGateway : IPersonApiGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _getPersonApiRoute;
        private readonly string _getPersonApiToken;

        private const string PersonApiUrl = "PersonApiUrl";
        private const string PersonApiToken = "PersonApiToken";
        private readonly static JsonSerializerOptions _jsonOptions = JsonOptions.CreateJsonOptions();

        public PersonApiGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _getPersonApiRoute = configuration.GetValue<string>(PersonApiUrl)?.TrimEnd('/');
            if (string.IsNullOrEmpty(_getPersonApiRoute) || !Uri.IsWellFormedUriString(_getPersonApiRoute, UriKind.Absolute))
                throw new ArgumentException($"Configuration does not contain a setting value for the key {PersonApiUrl}.");

            _getPersonApiToken = configuration.GetValue<string>(PersonApiToken);
            if (string.IsNullOrEmpty(_getPersonApiToken))
                throw new ArgumentException($"Configuration does not contain a setting value for the key {PersonApiToken}.");
        }

        [LogCall]
        public async Task<Person> GetPersonByIdAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            var getPersonRoute = $"{_getPersonApiRoute}/persons/{id}";

            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(_getPersonApiToken);
            var response = await client.GetAsync(new Uri(getPersonRoute))
                                       .ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.NotFound)
                return null;

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonSerializer.Deserialize<Person>(responseBody, _jsonOptions);

            throw new GetPersonException(id, response.StatusCode, responseBody);
        }
    }
}
