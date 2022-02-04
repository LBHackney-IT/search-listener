using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories.QueryableFactories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class AddPersonToTenureSteps : BaseSteps
    {
        private readonly PersonFactory _personFactory = new PersonFactory();
        private readonly TenuresFactory _tenuresFactory = new TenuresFactory();

        public AddPersonToTenureSteps()
        {
            _eventType = EventTypes.PersonAddedToTenureEvent;
        }

        public async Task WhenTheFunctionIsTriggered(Guid tenureId, EventData eventData, string eventType)
        {
            var eventMsg = CreateEvent(tenureId, eventType);
            eventMsg.EventData = eventData;
            await TriggerFunction(CreateMessage(eventMsg));
        }

        public void ThenAPersonNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<Person>));
            (_lastException as EntityNotFoundException<Person>).Id.Should().Be(id);
        }

        public async Task ThenTheIndexIsUpdatedWithThePerson(
            Person person, TenureInformation tenure, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryablePerson>(person.Id, g => g.Index("persons"))
                                       .ConfigureAwait(false);

            var personInIndex = result.Source;
            personInIndex.Should().BeEquivalentTo(_personFactory.CreatePerson(person),
                                                  c => c.Excluding(x => x.Tenures)
                                                        .Excluding(x => x.PersonTypes));

            var newTenure = personInIndex.Tenures.FirstOrDefault(x => x.Id == tenure.Id);
            newTenure.Should().NotBeNull();
            newTenure.AssetFullAddress.Should().Be(tenure.TenuredAsset.FullAddress);
            newTenure.EndDate.Should().Be(tenure.EndOfTenureDate);
            newTenure.StartDate.Should().Be(tenure.StartOfTenureDate);
            newTenure.Type.Should().Be(tenure.TenureType.Description);

            personInIndex.PersonTypes.Should().Contain("Tenant");
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
            tenureInIndex.Should().BeEquivalentTo(_tenuresFactory.CreateQueryableTenure(tenure));
        }
    }
}
