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
        public void TenureNotFound(EventTypes eventType)
        {
            var id = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureDoesNotExist(id))
                .When(w => _steps.WhenTheFunctionIsTriggered(id, eventType))
                .Then(t => _steps.ThenATenureNotFoundExceptionIsThrown(id))
                .BDDfy();
        }

        [Fact]
        public void TenureCreatedAddedToIndex()
        {
            var id = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureExists(id))
                .And(h => _esFixture.GivenATenureIsNotIndexed(TenureApiFixture.ResponseObject))
                .When(w => _steps.WhenTheFunctionIsTriggered(id, EventTypes.TenureCreatedEvent))
                .Then(t => _steps.ThenTheIndexIsUpdatedWithTheTenure(TenureApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}
