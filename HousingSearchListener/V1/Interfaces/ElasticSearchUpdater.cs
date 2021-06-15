using System;
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
        public ElasticSearchUpdater(IServiceCollection services) : base(services)
        {

        }

        public async Task Update(SQSEvent sqsEvent)
        {
            var httpHandler = ServiceProvider.GetService<IHttpHandler>();
            var personMessageFactory = ServiceProvider.GetService<IPersonMessageFactory>();

            foreach (var record in sqsEvent.Records)
            {
                var personCreatedMessage = personMessageFactory.Create(record);

                var url = $"{Environment.GetEnvironmentVariable("PersonApiUrl")}/persons/{personCreatedMessage.EntityId}";

                httpHandler.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(Environment.GetEnvironmentVariable("PersonApiToken"));
                var esPersonFactory = ServiceProvider.GetService<IESPersonFactory>();
                var esHelper = ServiceProvider.GetService<IElasticSearchHelper>();

                var result = await httpHandler.GetAsync(url);
                var personString = result.Content.ReadAsStringAsync().Result;
                var person = JsonConvert.DeserializeObject<Person>(personString);

                Logger.Log(LogLevel.Information, $"{personCreatedMessage.EntityId.ToString()}, {url}, {Environment.GetEnvironmentVariable("PersonApiToken")}, {personString}");

                var esPerson = esPersonFactory.Create(person);

                await esHelper.Create(esPerson);
            }
        }
    }
}