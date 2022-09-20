using Hackney.Shared.HousingSearch.Domain.Process;
using HousingSearchListener.Tests.V1.E2ETests.Steps;
using HousingSearchListener.V1.Boundary;
using System;
using TestStack.BDDfy;
using Xunit;

namespace HousingSearchListener.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "SQS Tenure Listener",
        IWant = "a function to process the Process updated messages",
        SoThat = "The process details are updated in the index correctly")]
    [Collection("ElasticSearch collection")]
    public class UpdateProcessTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly UpdateProcessSteps _steps;

        public UpdateProcessTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _steps = new UpdateProcessSteps();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
            }
        }

        [Theory]
        [InlineData(EventTypes.ProcessUpdatedEvent)]
        public void InvalidEventData(string eventType)
        {
            var processId = Guid.NewGuid();

            this.Given(g => _esFixture.GivenTheProcessIsIndexed(processId))
                .Given(g => _steps.GivenTheEventDataDoesNotContainStateChangeData())
                .When(w => _steps.WhenTheFunctionIsTriggered(processId, eventType))
                .Then(t => _steps.ThenAnInvalidEventDataTypeExceptionIsThrown<ProcessStateChangeData>())
                .BDDfy();
        }

        [Theory]
        [InlineData(EventTypes.ProcessUpdatedEvent)]
        public void ProcessNotIndexed(string eventType)
        {
            var processId = Guid.NewGuid();

            this.Given(g => _esFixture.GivenTheProcessIsNotIndexed(processId))
                .Given(g => _steps.GivenTheEventDataContainsStateChangeData())
                .When(w => _steps.WhenTheFunctionIsTriggered(processId, eventType))
                .Then(t => _steps.ThenAProcessNotIndexedExceptionIsThrown(processId.ToString()))
                .BDDfy();
        }

        [Theory]
        [InlineData(EventTypes.ProcessUpdatedEvent)]
        public void ProcessUpdatedAndSavedToIndex(string eventType)
        {
            var processId = Guid.NewGuid();

            this.Given(g => _esFixture.GivenTheProcessIsIndexed(processId))
                .Given(g => _steps.GivenTheEventDataContainsStateChangeData())
                .When(w => _steps.WhenTheFunctionIsTriggered(processId, eventType))
                .Then(t => _steps.ThenTheIndexIsUpdatedWithTheProcess(processId, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }

}