using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Moq;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class AddPersonToTenureSteps : BaseSteps
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly ESEntityFactory _entityFactory = new ESEntityFactory();
        private Exception _lastException;
        protected readonly Guid _correlationId = Guid.NewGuid();

        public AddPersonToTenureSteps()
        { }

        private SQSEvent.SQSMessage CreateMessage(Guid personId, EventData eventData, string eventType = EventTypes.PersonAddedToTenureEvent)
        {
            var personSns = _fixture.Build<EntityEventSns>()
                                    .With(x => x.EntityId, personId)
                                    .With(x => x.EventType, eventType)
                                    .With(x => x.EventData, eventData)
                                    .With(x => x.CorrelationId, _correlationId)
                                    .Create();

            var msgBody = JsonSerializer.Serialize(personSns, _jsonOptions);
            return _fixture.Build<SQSEvent.SQSMessage>()
                           .With(x => x.Body, msgBody)
                           .With(x => x.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>())
                           .Create();
        }

        public async Task WhenTheFunctionIsTriggered(Guid tenureId, EventData eventData, string eventType)
        {
            var mockLambdaLogger = new Mock<ILambdaLogger>();
            ILambdaContext lambdaContext = new TestLambdaContext()
            {
                Logger = mockLambdaLogger.Object
            };

            var msg = CreateMessage(tenureId, eventData, eventType);
            var sqsEvent = _fixture.Build<SQSEvent>()
                                   .With(x => x.Records, new List<SQSEvent.SQSMessage> { msg })
                                   .Create();

            Func<Task> func = async () =>
            {
                var fn = new HousingSearchListener();
                await fn.FunctionHandler(sqsEvent, lambdaContext).ConfigureAwait(false);
            };

            _lastException = await Record.ExceptionAsync(func);
        }

        public void ThenTheCorrelationIdWasUsedInTheApiCall(string receivedCorrelationId)
        {
            receivedCorrelationId.Should().Be(_correlationId.ToString());
        }

        public void ThenAPersonNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<Person>));
            (_lastException as EntityNotFoundException<Person>).Id.Should().Be(id);
        }

        public async Task ThenTheIndexIsUpdatedWithThePerson(
            Person person, TenureInformation tenure, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryablePerson>(person.Id, g => g.Index("persons"))
                                       .ConfigureAwait(false);

            var personInIndex = result.Source;
            personInIndex.Should().BeEquivalentTo(_entityFactory.CreatePerson(person),
                                                  c => c.Excluding(x => x.Tenures)
                                                        .Excluding(x => x.PersonTypes));

            var newTenure = personInIndex.Tenures.FirstOrDefault(x => x.Id == tenure.Id);
            newTenure.Should().NotBeNull();
            newTenure.AssetFullAddress.Should().Be(tenure.TenuredAsset.FullAddress);
            newTenure.EndDate.Should().Be(tenure.EndOfTenureDate);
            newTenure.StartDate.Should().Be(tenure.StartOfTenureDate);
            newTenure.Type.Should().Be(tenure.TenureType.Description);

            personInIndex.PersonTypes.Should().Contain("Tenant");            
        }

        public void ThenATenureNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<TenureInformation>));
            (_lastException as EntityNotFoundException<TenureInformation>).Id.Should().Be(id);
        }

        public async Task ThenTheIndexIsUpdatedWithTheTenure(
            TenureInformation tenure, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableTenure>(tenure.Id, g => g.Index("tenures"))
                                       .ConfigureAwait(false);

            var tenureInIndex = result.Source;
            tenureInIndex.Should().BeEquivalentTo(_entityFactory.CreateQueryableTenure(tenure));
        }
    }
}
