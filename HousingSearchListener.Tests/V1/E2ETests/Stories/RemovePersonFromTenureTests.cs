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
        IWant = "a function to process the RemovePersonFromTenure message",
        SoThat = "The tenure and person details are set in the respective indexes")]
    [Collection("ElasticSearch collection")]
    public class RemovePersonFromTenureTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly PersonApiFixture _personApiFixture;
        private readonly TenureApiFixture _tenureApiFixture;

        private readonly RemovePersonFromTenureSteps _steps;

        public RemovePersonFromTenureTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _personApiFixture = new PersonApiFixture();
            _tenureApiFixture = new TenureApiFixture();
            _steps = new RemovePersonFromTenureSteps();
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
                _personApiFixture.Dispose();
                _tenureApiFixture.Dispose();

                _disposed = true;
            }
        }

        [Fact]
        public void TenureNotFound()
        {
            var tenureId = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureDoesNotExist(tenureId))
                .When(w => _steps.WhenTheFunctionIsTriggered(tenureId, _tenureApiFixture.MessageEventData, EventTypes.PersonRemovedFromTenureEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationId))
                .Then(t => _steps.ThenATenureNotFoundExceptionIsThrown(tenureId))
                .BDDfy();
        }

        [Fact]
        public void PersonNotFound()
        {
            var tenureId = Guid.NewGuid();
            var removedPersonId = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureExists(tenureId))
                .And(g => _tenureApiFixture.GivenAPersonWasRemoved(removedPersonId))
                .And(g => _personApiFixture.GivenThePersonDoesNotExist(removedPersonId))
                .When(w => _steps.WhenTheFunctionIsTriggered(tenureId, _tenureApiFixture.MessageEventData, EventTypes.PersonRemovedFromTenureEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_personApiFixture.ReceivedCorrelationId))
                .Then(t => _steps.ThenAPersonNotFoundExceptionIsThrown(removedPersonId))
                .BDDfy();
        }

        [Fact]
        public void TenureAndPersonIndexesUpdated()
        {
            var tenureId = Guid.NewGuid();
            var removedPersonId = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureExists(tenureId))
                .And(g => _tenureApiFixture.GivenAPersonWasRemoved(removedPersonId))
                .And(g => _personApiFixture.GivenThePersonExistsWithTenure(removedPersonId, tenureId))
                .And(g => _esFixture.GivenTheOtherPersonTenuresExist(_personApiFixture.ResponseObject, tenureId))
                .When(w => _steps.WhenTheFunctionIsTriggered(tenureId, _tenureApiFixture.MessageEventData, EventTypes.PersonRemovedFromTenureEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_personApiFixture.ReceivedCorrelationId))
                .Then(t => _steps.ThenTheIndexedTenureHasThePersonRemoved(_tenureApiFixture.ResponseObject,
                                                                          removedPersonId, _esFixture.ElasticSearchClient))
                .Then(t => _steps.ThenTheIndexedPersonHasTheTenureRemoved(_personApiFixture.ResponseObject, tenureId, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}
