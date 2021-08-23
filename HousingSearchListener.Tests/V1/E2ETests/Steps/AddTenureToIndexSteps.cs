using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Moq;
using Nest;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class AddTenureToIndexSteps : BaseSteps
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly ESEntityFactory _entityFactory = new ESEntityFactory();
        private Exception _lastException;

        public AddTenureToIndexSteps()
        { }

        private SQSEvent.SQSMessage CreateMessage(Guid tenureId, string eventType = EventTypes.TenureCreatedEvent)
        {
            var tenureSns = _fixture.Build<EntityEventSns>()
                                    .With(x => x.EntityId, tenureId)
                                    .With(x => x.EventType, eventType)
                                    .Create();

            var msgBody = JsonSerializer.Serialize(tenureSns, _jsonOptions);
            return _fixture.Build<SQSEvent.SQSMessage>()
                           .With(x => x.Body, msgBody)
                           .With(x => x.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>())
                           .Create();
        }

        public async Task WhenTheFunctionIsTriggered(Guid tenureId, string eventType)
        {
            var mockLambdaLogger = new Mock<ILambdaLogger>();
            ILambdaContext lambdaContext = new TestLambdaContext()
            {
                Logger = mockLambdaLogger.Object
            };

            var sqsEvent = _fixture.Build<SQSEvent>()
                                   .With(x => x.Records, new List<SQSEvent.SQSMessage> { CreateMessage(tenureId, eventType) })
                                   .Create();

            Func<Task> func = async () =>
            {
                var fn = new HousingSearchListener();
                await fn.FunctionHandler(sqsEvent, lambdaContext).ConfigureAwait(false);
            };

            _lastException = await Record.ExceptionAsync(func);
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
            var result = await esClient.GetAsync<ESTenure>(tenure.Id, g => g.Index("tenures"))
                                       .ConfigureAwait(false);

            var tenureInIndex = result.Source;
            tenureInIndex.Should().BeEquivalentTo(_entityFactory.CreateTenure(tenure));

            _cleanup.Add(async () => await esClient.DeleteAsync(new DeleteRequest("tenures", tenureInIndex.Id))
                                                   .ConfigureAwait(false));
        }
    }
}
