using FluentAssertions;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Nest;
using System;
using System.Threading.Tasks;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class RemovePersonFromTenureSteps : BaseSteps
    {
        private readonly ESEntityFactory _entityFactory = new ESEntityFactory();

        public RemovePersonFromTenureSteps()
        {
            _eventType = EventTypes.PersonRemovedFromTenureEvent;
        }

        public async Task WhenTheFunctionIsTriggered(Guid tenureId, EventData eventData, string eventType)
        {
            var eventMsg = CreateEvent(tenureId, eventType);
            eventMsg.EventData = eventData;
            await TriggerFunction(CreateMessage(eventMsg));
        }

        public void ThenTheCorrelationIdWasUsedInTheApiCall(string receivedCorrelationId)
        {
            receivedCorrelationId.Should().Be(_correlationId.ToString());
        }

        public void ThenAPersonNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<Person>));
            (_lastException as EntityNotFoundException<Person>).Id.Should().Be(id);
        }

        public async Task ThenTheIndexIsUpdatedWithThePerson(
            Person person, Guid tenureId, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryablePerson>(person.Id, g => g.Index("persons"))
                                       .ConfigureAwait(false);

            var personInIndex = result.Source;
            personInIndex.Should().BeEquivalentTo(_entityFactory.CreatePerson(person), c => c.Excluding(y => y.Tenures));
            personInIndex.Tenures.Should().NotContain(x => x.Id == tenureId.ToString());
        }

        public void ThenATenureNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<TenureInformation>));
            (_lastException as EntityNotFoundException<TenureInformation>).Id.Should().Be(id);
        }

        public async Task ThenTheIndexIsUpdatedWithTheTenure(
            TenureInformation tenure, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableTenure>(tenure.Id, g => g.Index("tenures"))
                                       .ConfigureAwait(false);

            var tenureInIndex = result.Source;
            tenureInIndex.Should().BeEquivalentTo(_entityFactory.CreateQueryableTenure(tenure));
        }

        public async Task ThenTheIndexedTenureHasThePersonRemoved(
            TenureInformation tenure, Guid personId, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableTenure>(tenure.Id, g => g.Index("tenures"))
                                       .ConfigureAwait(false);

            var tenureInIndex = result.Source;
            tenureInIndex.Should().BeEquivalentTo(_entityFactory.CreateQueryableTenure(tenure), c => c.Excluding(y => y.HouseholdMembers));
            tenureInIndex.HouseholdMembers.Should().NotContain(x => x.Id == personId.ToString());
        }

        public async Task ThenTheIndexedPersonHasTheTenureRemoved(
            Person person, Guid tenureId, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryablePerson>(person.Id, g => g.Index("persons"))
                                       .ConfigureAwait(false);

            var personInIndex = result.Source;
            personInIndex.Should().BeEquivalentTo(_entityFactory.CreatePerson(person),
                                                  c => c.Excluding(y => y.Tenures).Excluding(z => z.PersonTypes));
            personInIndex.Tenures.Should().HaveCount(person.Tenures.Count - 1);
            personInIndex.Tenures.Should().NotContain(x => x.Id == tenureId.ToString());
            personInIndex.PersonTypes.Should().HaveCount(person.PersonTypes.Count - 1);
            personInIndex.PersonTypes.Should().NotContain("Freeholder");
        }
    }
}
