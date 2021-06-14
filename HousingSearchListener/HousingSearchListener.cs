using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using HousingSearchListener.Gateways;
using HousingSearchListener.Infrastructure;
using HousingSearchListener.V1.Domain;
using HousingSearchListener.V1.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HousingSearchListener
{
    public class HousingSearchListener : BaseFunction
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SNSEvent snsEvent)
        {
            Environment.SetEnvironmentVariable("PersonApiUrl", "http://127.0.0.1");
            //var httpClientFactory = ServiceProvider.GetService<IHttpClientFactory>();
            var personMessageFactory = ServiceProvider.GetService<IPersonMessageFactory>();

            foreach (var record in snsEvent.Records)
            {
                var personCreatedMessage = personMessageFactory.Create(record);

                var httpClient = new HttpClient();

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