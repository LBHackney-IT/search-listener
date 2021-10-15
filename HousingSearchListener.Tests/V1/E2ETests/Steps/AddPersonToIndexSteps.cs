using FluentAssertions;
using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class AddPersonToIndexSteps : BaseSteps
    {
        private readonly ESEntityFactory _entityFactory = new ESEntityFactory();

        public AddPersonToIndexSteps()
        {
            _eventType = EventTypes.PersonCreatedEvent;
        }

        public async Task WhenTheFunctionIsTriggered(Guid personId, string eventType)
        {
            var eventMsg = CreateEvent(personId, eventType);
            await TriggerFunction(CreateMessage(eventMsg));
        }

        public void ThenAPersonNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<Person>));
            (_lastException as EntityNotFoundException<Person>).Id.Should().Be(id);
        }

        public async Task ThenTheIndexIsUpdatedWithThePerson(
            Person person, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryablePerson>(person.Id, g => g.Index("persons"))
                                       .ConfigureAwait(false);

            var personInIndex = result.Source;
            personInIndex.Should().BeEquivalentTo(_entityFactory.CreatePerson(person));
        }

        public async Task ThenTheIndexIsUpdatedWithTheUpdatedPersonTenure(
            Person person, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableTenure>(person.Tenures.First().Id, g => g.Index("tenures"))
                .ConfigureAwait(false);

            var tenureInIndex = result.Source;
            tenureInIndex.HouseholdMembers.First().Id.Should().Be(person.Id);
            tenureInIndex.HouseholdMembers.First().FullName.Should().Be(person.FullName);
        }
    }
}
