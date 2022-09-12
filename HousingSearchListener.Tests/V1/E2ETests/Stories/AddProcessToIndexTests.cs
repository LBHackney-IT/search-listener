using Hackney.Shared.HousingSearch.Domain.Process;
using HousingSearchListener.Tests.V1.E2ETests.Fixtures;
using HousingSearchListener.Tests.V1.E2ETests.Steps;
using HousingSearchListener.V1.Boundary;
using System;
using TestStack.BDDfy;
using Xunit;

namespace HousingSearchListener.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "SQS Tenure Listener",
        IWant = "a function to process the process started message",
        SoThat = "The process details are added in the index")]
    [Collection("ElasticSearch collection")]
    public class AddProcessToIndexTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly ProcessesApiFixture _processesApiFixture;
        private readonly TenureApiFixture _tenureApiFixture;
        private readonly PersonApiFixture _personApiFixture;
        private readonly AssetApiFixture _assetApiFixture;
        private readonly AddProcessToIndexSteps _steps;

        public AddProcessToIndexTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _processesApiFixture = new ProcessesApiFixture();
            _tenureApiFixture = new TenureApiFixture();
            _personApiFixture = new PersonApiFixture();
            _assetApiFixture = new AssetApiFixture();

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
                _processesApiFixture.Dispose();
                _assetApiFixture.Dispose();
                _tenureApiFixture.Dispose();
                _personApiFixture.Dispose();

                _disposed = true;
            }
        }

        [Fact]
        public void ProcessNotFound()
        {
            var ProcessId = Guid.NewGuid();
            this.Given(g => _processesApiFixture.GivenTheProcessDoesNotExist(ProcessId))
                .When(w => _steps.WhenTheFunctionIsTriggered(ProcessId, EventTypes.ProcessStartedEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_processesApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenAProcessNotFoundExceptionIsThrown(ProcessId))
                .BDDfy();
        }

        [Theory]
        [InlineData(TargetType.asset)]
        [InlineData(TargetType.tenure)]
        [InlineData(TargetType.person)]
        public void TargetEntityNotFound(TargetType targetType)
        {
            var processId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            this.Given(g => _processesApiFixture.GivenTheProcessExists(processId, targetId, targetType))
                .Given(g => _steps.GivenTheTargetEntityDoesNotExist(_assetApiFixture, _personApiFixture, _tenureApiFixture, targetId, targetType))
                .When(w => _steps.WhenTheFunctionIsTriggered(processId, EventTypes.ProcessStartedEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCalls(_processesApiFixture, _assetApiFixture, _personApiFixture, _tenureApiFixture))
                .Then(t => _steps.ThenATargetEntityNotFoundExceptionIsThrown(targetId, targetType))
                .BDDfy();
        }

        [Theory]
        [InlineData(TargetType.asset)]
        [InlineData(TargetType.tenure)]
        [InlineData(TargetType.person)]
        public void ProcessIsAddedToIndex(TargetType targetType)
        {
            var processId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            this.Given(g => _processesApiFixture.GivenTheProcessExists(processId, targetId, targetType))
                .Given(g => _steps.GivenTheTargetEntityExists(_assetApiFixture, _personApiFixture, _tenureApiFixture, targetId, targetType))
                .When(w => _steps.WhenTheFunctionIsTriggered(processId, EventTypes.ProcessStartedEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCalls(_processesApiFixture, _assetApiFixture, _personApiFixture, _tenureApiFixture))
                .Then(t => _steps.ThenTheIndexIsUpdatedWithTheProcess(_processesApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}
