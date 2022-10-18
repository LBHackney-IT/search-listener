using SharedProcess = Hackney.Shared.HousingSearch.Domain.Process;
using Hackney.Shared.Processes.Domain;
using HousingSearchListener.Tests.V1.E2ETests.Fixtures;
using HousingSearchListener.Tests.V1.E2ETests.Steps;
using HousingSearchListener.V1.Boundary;
using System;
using TestStack.BDDfy;
using Xunit;
using Process = Hackney.Shared.Processes.Domain.Process;

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
        private readonly TenureApiFixture _tenureApiFixture;
        private readonly PersonApiFixture _personApiFixture;
        private readonly AssetApiFixture _assetApiFixture;
        private readonly AddProcessToIndexSteps _steps;

        public AddProcessToIndexTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
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
                _assetApiFixture?.Dispose();
                _tenureApiFixture?.Dispose();
                _personApiFixture?.Dispose();

                _disposed = true;
            }
        }

        [Fact]
        public void InvalidEventData()
        {
            var processId = Guid.NewGuid();
            this.Given(g => _steps.GivenTheMessageDoesNotContainAProcess(processId))
                .When(w => _steps.WhenTheFunctionIsTriggered(processId, EventTypes.ProcessStartedEvent))
                .Then(t => _steps.ThenAnInvalidEventDataTypeExceptionIsThrown<SharedProcess.Process>())
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

            this.Given(g => _steps.GivenTheMessageContainsAProcess(processId, targetId, targetType))
                .Given(g => _steps.GivenTheTargetEntityDoesNotExist(_assetApiFixture, _personApiFixture, _tenureApiFixture, targetId, targetType))
                .When(w => _steps.WhenTheFunctionIsTriggered(processId, EventTypes.ProcessStartedEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_assetApiFixture, _personApiFixture, _tenureApiFixture))
                .Then(t => _steps.ThenATargetEntityNotFoundExceptionIsThrown(targetId, targetType))
                .BDDfy();
        }

        [Theory]
        [InlineData(TargetType.asset)]
        [InlineData(TargetType.tenure)]
        [InlineData(TargetType.person)]
        public void ProcessWithoutTargetEntityIsAddedToIndex(TargetType targetType)
        {
            var processId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            this.Given(g => _steps.GivenTheMessageContainsAProcess(processId, targetId, targetType))
                .Given(g => _steps.GivenTheTargetEntityExists(_assetApiFixture, _personApiFixture, _tenureApiFixture, targetId, targetType))
                .When(w => _steps.WhenTheFunctionIsTriggered(processId, EventTypes.ProcessStartedEvent))
                .Then(t => _steps.ThenNoExceptionsAreThrown())
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_assetApiFixture, _personApiFixture, _tenureApiFixture))
                .Then(t => _steps.ThenTheIndexIsUpdatedWithTheProcess(_esFixture.ElasticSearchClient))
                .BDDfy();
        }

        [Theory]
        [InlineData(TargetType.asset)]
        [InlineData(TargetType.tenure)]
        [InlineData(TargetType.person)]
        public void ProcessWithTargetEntityIsAddedToIndex(TargetType targetType)
        {
            var processId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            this.Given(g => _steps.GivenTheMessageContainsAProcess(processId, targetId, targetType))
                .Given(g => _steps.GivenTheProcessContainsATargetEntity())
                .When(w => _steps.WhenTheFunctionIsTriggered(processId, EventTypes.ProcessStartedEvent))
                .Then(t => _steps.ThenNoExceptionsAreThrown())
                .Then(t => _steps.ThenTheIndexIsUpdatedWithTheProcess(_esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}
