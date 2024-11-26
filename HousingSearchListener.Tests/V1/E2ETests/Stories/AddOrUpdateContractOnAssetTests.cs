using HousingSearchListener.Tests.V1.E2ETests.Fixtures;
using HousingSearchListener.Tests.V1.E2ETests.Steps;
using HousingSearchListener.V1.Boundary;
using System;
using TestStack.BDDfy;
using Xunit;

namespace HousingSearchListener.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "SQS Contract Listener",
        IWant = "a function to process the Contract added or updated messages",
        SoThat = "The Contract details are updated on the Asset in the index")]
    [Collection("ElasticSearch collection")]
    public class AddOrUpdateContractOnAssetTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly AssetApiFixture _AssetApiFixture;
        private readonly ContractApiFixture _ContractApiFixture;

        private readonly AddOrUpdateContractOnAssetTestsSteps _steps;

        public AddOrUpdateContractOnAssetTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _AssetApiFixture = new AssetApiFixture();
            _ContractApiFixture = new ContractApiFixture();

            _steps = new AddOrUpdateContractOnAssetTestsSteps();
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
                _ContractApiFixture.Dispose();

                _disposed = true;
            }
        }

        [Theory]
        [InlineData(EventTypes.ContractCreatedEvent)]
        [InlineData(EventTypes.ContractUpdatedEvent)]
        public void ContractNotFound(string eventType)
        {
            var contractId = Guid.NewGuid();
            this.Given(g => _ContractApiFixture.GivenTheContractDoesNotExist(contractId))
                .When(w => _steps.WhenTheFunctionIsTriggered(contractId, eventType, "assetId.ToString()"))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_ContractApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenAContractNotFoundExceptionIsThrown(contractId))
                .BDDfy();
        }

        [Theory]
        [InlineData(EventTypes.ContractCreatedEvent)]
        [InlineData(EventTypes.ContractUpdatedEvent)]
        public void AssetNotFound(string eventType)
        {
            var contractId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            this.Given(g => _ContractApiFixture.GivenTheContractExists(contractId, assetId))
                .And(g => _AssetApiFixture.GivenTheAssetDoesNotExist(assetId))
                .When(w => _steps.WhenTheFunctionIsTriggered(contractId, eventType, ""))
                .Then(t => _steps.ThenAnAssetNotFoundExceptionIsThrown(assetId))
                .BDDfy();
        }

        [Theory]
        [InlineData(EventTypes.ContractCreatedEvent)]
        [InlineData(EventTypes.ContractUpdatedEvent)]
        public void ContractAddedToAsset(string eventType)
        {
            var contractId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            this.Given(g => _ContractApiFixture.GivenTheContractExists(contractId, assetId))
                .And(g => _AssetApiFixture.GivenTheAssetExists(assetId))
                .And(g => _esFixture.GivenAnAssetIsIndexed(assetId.ToString()))
                .When(w => _steps.WhenTheFunctionIsTriggered(contractId, eventType, assetId.ToString()))
                .Then(t => _steps.ThenTheAssetInTheIndexIsUpdatedWithTheContract(_AssetApiFixture.ResponseObject,
                    _ContractApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}