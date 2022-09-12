using HousingSearchListener.Tests.V1.E2ETests.Fixtures;
using HousingSearchListener.Tests.V1.E2ETests.Steps;
using HousingSearchListener.V1.Boundary;
using System;
using System.Collections.Generic;
using System.Text;
using TestStack.BDDfy;
using Xunit;

namespace HousingSearchListener.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "SQS Tenure Listener",
        IWant = "a function to process the Process updated messages",
        SoThat = "The process details are set in the index")]
    [Collection("ElasticSearch collection")]
    public class UpdateProcessTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly ProcessesApiFixture _ProcessesApiFixture;

        private readonly AddProcessToIndexSteps _steps;

        public UpdateProcessTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _ProcessesApiFixture = new ProcessesApiFixture();

            _steps = new AddProcessToIndexSteps();
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
                _ProcessesApiFixture.Dispose();

                _disposed = true;
            }
        }

        [Theory]
        [InlineData(EventTypes.ProcessUpdatedEvent)]
        public void ProcessNotFound(string eventType)
        {
            var ProcessId = Guid.NewGuid();
            this.Given(g => _ProcessesApiFixture.GivenTheProcessDoesNotExist(ProcessId))
                .When(w => _steps.WhenTheFunctionIsTriggered(ProcessId, eventType))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_ProcessesApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenAProcessNotFoundExceptionIsThrown(ProcessId))
                .BDDfy();
        }

        [Fact]
        public void ProcessUpdateAndAddedToIndex()
        {
            var ProcessId = Guid.NewGuid();
            this.Given(g => _ProcessesApiFixture.GivenTheProcessExists(ProcessId))
                .When(w => _steps.WhenTheFunctionIsTriggered(ProcessId, EventTypes.ProcessUpdatedEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_ProcessesApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenTheIndexIsUpdatedWithTheProcess(_ProcessesApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }

}