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
        IWant = "a function to process the Asset created and updated messages",
        SoThat = "The Asset details are set in the index")]
    [Collection("ElasticSearch collection")]
    public class UpdateAssetTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly AssetApiFixture _AssetApiFixture;

        private readonly AddAssetToIndexSteps _steps;

        public UpdateAssetTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _AssetApiFixture = new AssetApiFixture();

            _steps = new AddAssetToIndexSteps();
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
                _AssetApiFixture.Dispose();

                _disposed = true;
            }
        }

        [Theory]
        [InlineData(EventTypes.AssetUpdatedEvent)]
        public void AssetNotFound(string eventType)
        {
            var AssetId = Guid.NewGuid();
            this.Given(g => _AssetApiFixture.GivenTheAssetDoesNotExist(AssetId))
                .When(w => _steps.WhenTheFunctionIsTriggered(AssetId, eventType))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_AssetApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenAnAssetNotFoundExceptionIsThrown(AssetId))
                .BDDfy();
        }

        [Fact]
        public void AssetUpdateAndAddedToIndex()
        {
            var AssetId = Guid.NewGuid();
            this.Given(g => _AssetApiFixture.GivenTheAssetExists(AssetId))
                .When(w => _steps.WhenTheFunctionIsTriggered(AssetId, EventTypes.AssetCreatedEvent))
                .When(w => _steps.WhenTheFunctionIsTriggered(AssetId, EventTypes.AssetUpdatedEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_AssetApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenTheIndexIsUpdatedWithTheAsset(_AssetApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}
