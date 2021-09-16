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
        IWant = "a function to process the person created and updated messages",
        SoThat = "The person details are set in the index")]
    [Collection("ElasticSearch collection")]
    public class AddPersonToIndexTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly PersonApiFixture _personApiFixture;

        private readonly AddPersonToIndexSteps _steps;

        public AddPersonToIndexTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _personApiFixture = new PersonApiFixture();

            _steps = new AddPersonToIndexSteps();
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

                _disposed = true;
            }
        }

        [Theory]
        [InlineData(EventTypes.PersonCreatedEvent)]
        [InlineData(EventTypes.PersonUpdatedEvent)]
        public void PersonNotFound(EventTypes eventType)
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonDoesNotExist(personId))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId, eventType))
                .Then(t => _steps.ThenAPersonNotFoundExceptionIsThrown(personId))
                .BDDfy();
        }

        [Fact]
        public void PersonCreatedAddedToIndex()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExists(personId))
                .And(h => _esFixture.GivenAPersonIsNotIndexed(PersonApiFixture.ResponseObject))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId, EventTypes.PersonCreatedEvent))
                .Then(t => _steps.ThenTheIndexIsUpdatedWithThePerson(PersonApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .BDDfy();
        }

        [Fact]
        public void PersonUpdateInIndex()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExists(personId))
                .And(h => _esFixture.GivenAPersonIsIndexedWithDifferentInfo(PersonApiFixture.ResponseObject))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId, EventTypes.PersonUpdatedEvent))
                .Then(t => _steps.ThenTheIndexIsUpdatedWithThePerson(PersonApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}
