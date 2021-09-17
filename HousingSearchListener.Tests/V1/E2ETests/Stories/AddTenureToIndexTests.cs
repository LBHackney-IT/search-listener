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
        IWant = "a function to process the tenure created message",
        SoThat = "The tenure details are set in the index")]
    [Collection("ElasticSearch collection")]
    public class AddTenureToIndexTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly TenureApiFixture _tenureApiFixture;

        private readonly AddTenureToIndexSteps _steps;

        public AddTenureToIndexTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _tenureApiFixture = new TenureApiFixture();

            _steps = new AddTenureToIndexSteps();
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
                _tenureApiFixture.Dispose();

                _disposed = true;
            }
        }

        [Theory]
        [InlineData(EventTypes.TenureCreatedEvent)]
        [InlineData(EventTypes.TenureUpdatedEvent)]
        public void TenureNotFound(string eventType)
        {
            var id = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureDoesNotExist(id))
                .When(w => _steps.WhenTheFunctionIsTriggered(id, eventType))
                .Then(t => _steps.ThenATenureNotFoundExceptionIsThrown(id))
                .BDDfy();
        }

        [Theory]
        [InlineData(EventTypes.TenureCreatedEvent)]
        [InlineData(EventTypes.TenureUpdatedEvent)]
        public void TenureChangedAssetNotFoundInIndexThrowsException(string eventType)
        {
            var id = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureExists(id))
                .And(h => _esFixture.GivenATenureIsNotIndexed(TenureApiFixture.ResponseObject))
                .And(ih => _esFixture.GivenAnAssetIsNotIndexed(TenureApiFixture.ResponseObject.TenuredAsset.Id))
                .When(w => _steps.WhenTheFunctionIsTriggered(id, eventType))
                .Then(t => _steps.ThenAnAssetNotIndexedExceptionIsThrown(TenureApiFixture.ResponseObject.TenuredAsset.Id))
                .BDDfy();
        }

        [Fact]
        public void TenureCreatedIndexesUpdated()
        {
            var id = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureExists(id))
                .And(h => _esFixture.GivenATenureIsNotIndexed(TenureApiFixture.ResponseObject))
                .And(i => _esFixture.GivenAnAssetIsIndexed(TenureApiFixture.ResponseObject.TenuredAsset.Id))
                .When(w => _steps.WhenTheFunctionIsTriggered(id, EventTypes.TenureCreatedEvent))
                .Then(t => _steps.ThenTheTenureIndexIsUpdated(TenureApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .Then(t => _steps.ThenTheAssetIndexIsUpdatedWithTheTenure(TenureApiFixture.ResponseObject,
                                                                          _esFixture.AssetInIndex, 
                                                                          _esFixture.ElasticSearchClient))
                .BDDfy();
        }

        [Fact]
        public void TenureUpdatedIndexesUpdated()
        {
            var id = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureExists(id))
                .And(h => _esFixture.GivenATenureIsIndexedWithDifferentInfo(TenureApiFixture.ResponseObject))
                .And(i => _esFixture.GivenAnAssetIsIndexed(TenureApiFixture.ResponseObject.TenuredAsset.Id))
                .When(w => _steps.WhenTheFunctionIsTriggered(id, EventTypes.TenureUpdatedEvent))
                .Then(t => _steps.ThenTheTenureIndexIsUpdated(TenureApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .Then(t => _steps.ThenTheAssetIndexIsUpdatedWithTheTenure(TenureApiFixture.ResponseObject,
                                                                          _esFixture.AssetInIndex,
                                                                          _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}
