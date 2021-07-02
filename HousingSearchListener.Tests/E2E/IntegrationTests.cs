using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using FluentAssertions;
using HousingSearchListener.Tests.Stubs;
using HousingSearchListener.V1.Domain;
using HousingSearchListener.V1.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using Xunit;

namespace HousingSearchListener.Tests.E2E
{
    [Collection("ElasticSearch collection")]
    public class IntegrationTests
    {
        private ElasticSearchService _sut;
        private ServiceCollection _serviceCollection;
        private ServiceProvider _serviceProvider;
        private readonly Indices.ManyIndices _indices;


        public IntegrationTests()
        {
            SetupEnvVariables();

            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddSingleton<IHttpHandler, HttpHandlerStub>();

            _sut = new ElasticSearchService(_serviceCollection);
            _serviceProvider = _serviceCollection.BuildServiceProvider();

            _indices = Indices.Index(new List<IndexName> { "persons" });
        }

        private static void SetupEnvVariables()
        {
            if (Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL") == null)
                Environment.SetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL", "http://localhost:9200");

            if (Environment.GetEnvironmentVariable("PersonApiUrl") == null)
                Environment.SetEnvironmentVariable("PersonApiUrl", "http://fakehost");

            if (Environment.GetEnvironmentVariable("PersonApiToken") == null)
                Environment.SetEnvironmentVariable("PersonApiToken", "faketoken");
        }

        [Fact]
        public async Task GivenAnSnsTopicForAPersonCreated_WhenProcessingMessage_ShouldIndexNewPersonInElasticSearch()
        {
            //given
            var _esClient = _serviceProvider.GetService<IElasticClient>();
            var httpHandlerStub = _serviceProvider.GetService<IHttpHandler>();
            var personString = httpHandlerStub.GetAsync("").Result.Content.ReadAsStringAsync().Result;
            var person = JsonConvert.DeserializeObject<Person>(personString);
            person.Id = Guid.NewGuid().ToString();
            var result = await _esClient.SearchAsync<QueryablePerson>(x => x.Index(_indices)
                .Query(q => Create(q, new[] { $"*{person.FirstName}*", $"*{person.Surname}*" })));

            var sqsMessage =
                "{ \"id\": \"8e648f3d-9556-4896-8400-211cb1c5451b\", \"eventType\": \"PersonCreatedEvent\", \"sourceDomain\": \"Person\", \"sourceSystem\": " +
                "\"PersonAPI\", \"version\": \"v1\", \"correlationId\": \"f4d541d0-7c07-4524-8296-2d0d50cb58f4\", \"dateTime\": \"2021-05-17T11:59:57.25Z\", " +
                "\"user\": { \"id\": \"ac703d87-c100-40ec-90a0-dabf183e7377\", \"name\": \"Joe Bloggs\", \"email\": \"joe.bloggs@hackney.gov.uk\" }, " +
                "\"entityId\": \"" + person.Id + "\"}";

            result.Documents.Count.Should().Be(0);

            // when
            await _sut.Process(new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>()
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = sqsMessage
                    }
                }
            });

            result = await _esClient.SearchAsync<QueryablePerson>(x => x.Index(_indices)
                .Query(q => Create(q, new[] { $"*{person.FirstName}*", $"*{person.Surname}*" })));

            // then
            result.Documents.Count.Should().BeGreaterThan(0);
        }

        public QueryContainer Create(QueryContainerDescriptor<QueryablePerson> q, string[] listOfWildCardedWords)
        {
            var searchSurnames = q.QueryString(m =>
                m.Query(string.Join(' ', listOfWildCardedWords))
                    .Fields(f => f.Field(p => p.Firstname).Field(p => p.Surname))
                    .Type(TextQueryType.MostFields));

            return searchSurnames;
        }
    }
}
