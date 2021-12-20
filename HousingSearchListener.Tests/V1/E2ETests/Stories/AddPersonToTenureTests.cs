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
        IWant = "a function to process the AddPersonToTenure message",
        SoThat = "The tenure and person details are set in the respective indexes")]
    [Collection("ElasticSearch collection")]
    public class AddPersonToTenureTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly PersonApiFixture _personApiFixture;
        private readonly TenureApiFixture _tenureApiFixture;

        private readonly AddPersonToTenureSteps _steps;

        public AddPersonToTenureTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _personApiFixture = new PersonApiFixture();
            _tenureApiFixture = new TenureApiFixture();

            _steps = new AddPersonToTenureSteps();
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
                .When(w => _steps.WhenTheFunctionIsTriggered(tenureId, _tenureApiFixture.MessageEventData, EventTypes.PersonAddedToTenureEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenATenureNotFoundExceptionIsThrown(tenureId))
                .BDDfy();
        }

        [Fact]
        public void PersonNotFound()
        {
            var tenureId = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureExists(tenureId))
                .And(g => _personApiFixture.GivenThePersonDoesNotExist(_tenureApiFixture.AddedPersonId))
                .When(w => _steps.WhenTheFunctionIsTriggered(tenureId, _tenureApiFixture.MessageEventData, EventTypes.PersonAddedToTenureEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenAPersonNotFoundExceptionIsThrown(_tenureApiFixture.AddedPersonId))
                .BDDfy();
        }

        [Fact]
        public void TenureAndPersonIndexesUpdated()
        {
            var tenureId = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureExists(tenureId))
                .And(g => _personApiFixture.GivenThePersonExists(_tenureApiFixture.AddedPersonId))
                .When(w => _steps.WhenTheFunctionIsTriggered(tenureId, _tenureApiFixture.MessageEventData, EventTypes.PersonAddedToTenureEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenTheIndexIsUpdatedWithTheTenure(_tenureApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .Then(t => _steps.ThenTheIndexIsUpdatedWithThePerson(_personApiFixture.ResponseObject, _tenureApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}
