using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using HousingSearchListener.Gateways;
using HousingSearchListener.Infrastructure;
using HousingSearchListener.V1.Domain;
using HousingSearchListener.V1.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace HousingSearchListener
{
    public class ElasticSearchUpdater : BaseFunction, IElasticSearchUpdater
    {
        public ElasticSearchUpdater(IServiceCollection services) : base(services)
        {
            
        }

        public async Task Update(SNSEvent snsEvent)
        {
            var httpClientFactory = ServiceProvider.GetService<IHttpClientFactory>();
            var personMessageFactory = ServiceProvider.GetService<IPersonMessageFactory>();

            foreach (var record in snsEvent.Records)
            {
                var personCreatedMessage = personMessageFactory.Create(record);

                var httpClient = httpClientFactory.CreateClient();

                var url = QueryHelpers.AddQueryString(Environment.GetEnvironmentVariable("PersonApiUrl"),
                    "id", personCreatedMessage.EntityId.ToString());

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(Environment.GetEnvironmentVariable("PersonApiToken"));
                var esPersonFactory = ServiceProvider.GetService<IESPersonFactory>();
                var esHelper = ServiceProvider.GetService<IElasticSearchHelper>();

                var result = await httpClient.GetAsync(url);
                var person = JsonConvert.DeserializeObject<Person>(result.Content.ReadAsStringAsync().Result);
                var esPerson = esPersonFactory.Create(person);

            }
        }
    }
}