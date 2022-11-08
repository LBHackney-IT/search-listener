using AutoFixture;
using FluentAssertions;
using Hackney.Shared.HousingSearch.Domain.Process;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;
using Hackney.Shared.Processes.Sns;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Exceptions;
using Nest;
using System;
using System.Threading.Tasks;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class UpdateProcessSteps : BaseSteps
    {
        private readonly ESEntityFactory _entityFactory = new ESEntityFactory();
        private ProcessStateChangeData _stateChangeData;

        public UpdateProcessSteps()
        {
            _eventType = EventTypes.ProcessUpdatedEvent;
        }

        public void GivenTheEventDataDoesNotContainStateChangeData()
        {
            // do nothing
        }

        public void GivenTheEventDataContainsStateChangeData()
        {
            _stateChangeData = _fixture.Create<ProcessStateChangeData>();
        }

        public async Task WhenTheFunctionIsTriggered(Guid ProcessId, string eventType)
        {
            var eventMsg = CreateEvent(ProcessId, eventType);
            eventMsg.EventData.NewData = _stateChangeData;

            await TriggerFunction(CreateMessage(eventMsg));
        }

        public void ThenAProcessNotIndexedExceptionIsThrown(string id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotIndexedException<QueryableProcess>));
            (_lastException as EntityNotIndexedException<QueryableProcess>).Id.Should().Be(id);
        }

        public void ThenAnInvalidEventDataTypeExceptionIsThrown<T>() where T : class
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(InvalidEventDataTypeException<T>));
        }

        public async Task ThenTheIndexIsUpdatedWithTheProcess(Guid id, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableProcess>(id, g => g.Index("processes"))
                                       .ConfigureAwait(false);
            var process = result.Source;

            process.Should().NotBeNull();
            process.State.Should().Be(_stateChangeData.State);
            process.StateStartedAt.Should().NotBeNull();
            process.StateStartedAt.Should().Be(_stateChangeData.StateStartedAt.ToString("O"));
        }

        public void ThenNoExceptionsAreThrown()
        {
            _lastException.Should().BeNull();
        }
    }
}
