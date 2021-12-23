using HousingSearchListener.V1.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    /// <summary>
    /// Gateway to load data from other APIs
    /// </summary>
    [Obsolete("We need to use ApiGateway from package Hackney.Core.Http. This one will be used only in TransactionApiGateway to allow custom attributes")]
    public class NewtonsoftApiGateway : INewtonsoftApiGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public string ApiRoute { get; private set; }
        public string ApiToken { get; private set; }
        public string ApiName { get; private set; }
        public Dictionary<string, string> RequestHeaders { get; private set; }

        private bool _initialised = false;

        public NewtonsoftApiGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void Initialise(string apiName, string configKeyApiUrl, string configKeyApiToken, Dictionary<string, string> headers = null)
        {
            if (string.IsNullOrEmpty(apiName)) throw new ArgumentNullException(nameof(apiName));
            ApiName = apiName;

            var apiRoute = _configuration.GetValue<string>(configKeyApiUrl)?.TrimEnd('/');
            if (string.IsNullOrEmpty(apiRoute) || !Uri.IsWellFormedUriString(apiRoute, UriKind.Absolute))
                throw new ArgumentException($"Configuration does not contain a setting value for the key {configKeyApiUrl}.");
            ApiRoute = apiRoute;

            var apiToken = _configuration.GetValue<string>(configKeyApiToken);
            if (string.IsNullOrEmpty(apiToken))
                throw new ArgumentException($"Configuration does not contain a setting value for the key {configKeyApiToken}.");
            ApiToken = apiToken;

            RequestHeaders = headers ?? new Dictionary<string, string>();

            _initialised = true;
        }

        public async Task<T> GetByIdAsync<T>(string route, Guid id, Guid correlationId) where T : class
        {
            if (!_initialised) throw new InvalidOperationException("Initialise() must be called before any other calls are made");

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-correlation-id", correlationId.ToString());
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(ApiToken);
            foreach (var pair in RequestHeaders)
                client.DefaultRequestHeaders.Add(pair.Key, pair.Value);

            var response = await client.GetAsync(new Uri(route))
                                       .ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.NotFound)
                return null;

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(responseBody);

            throw new GetFromApiException(ApiName, route, client.DefaultRequestHeaders.ToList(),
                                          id, response.StatusCode, responseBody);
        }
    }
}
