using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using HousingSearchListener.Gateways;
using HousingSearchListener.Infrastructure;
using HousingSearchListener.V1.Domain;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HousingSearchListener.V1.Interfaces
{
    public class ElasticSearchUpdater : BaseFunction, IElasticSearchUpdater
    {
        private IESPersonFactory _esPersonFactory;
        private IElasticSearchHelper _esHelper;
        private IHttpHandler _httpHandler;
        private IPersonMessageFactory _personMessageFactory;

        public ElasticSearchUpdater(IServiceCollection services) : base(services)
        {
            _esPersonFactory = ServiceProvider.GetService<IESPersonFactory>();
            _esHelper = ServiceProvider.GetService<IElasticSearchHelper>();
            _httpHandler = ServiceProvider.GetService<IHttpHandler>();
            _personMessageFactory = ServiceProvider.GetService<IPersonMessageFactory>();
        }

        public async Task Update(SQSEvent sqsEvent)
        {
            foreach (var record in sqsEvent.Records)
            {
                var result = await GetPersonFromPersonApi(record);
                await UpdateEsIndexWithCreatedPerson(result);
            }
        }

        private async Task<HttpResponseMessage> GetPersonFromPersonApi(SQSEvent.SQSMessage record)
        {
            var personCreatedMessage = _personMessageFactory.Create(record);
            var personApiUrl = $"{Environment.GetEnvironmentVariable("PersonApiUrl")}/persons/{personCreatedMessage.EntityId}";

            _httpHandler.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(Environment.GetEnvironmentVariable("PersonApiToken"));

            var result = await _httpHandler.GetAsync(personApiUrl);

            return result;
        }

        private async Task UpdateEsIndexWithCreatedPerson(HttpResponseMessage result)
        {
            var personString = await result.Content.ReadAsStringAsync();
            var person = JsonConvert.DeserializeObject<Person>(personString);

            var esPerson = _esPersonFactory.Create(person);

            await _esHelper.Create(esPerson);
        }
    }
}