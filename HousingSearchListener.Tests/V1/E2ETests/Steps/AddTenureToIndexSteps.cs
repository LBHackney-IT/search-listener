using FluentAssertions;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class AddTenureToIndexSteps : BaseSteps
    {
        private readonly ESEntityFactory _entityFactory = new ESEntityFactory();

        public AddTenureToIndexSteps()
        {
            _eventType = EventTypes.TenureCreatedEvent;
        }

        public async Task WhenTheFunctionIsTriggered(Guid tenureId, string eventType)
        {
            var eventMsg = CreateEvent(tenureId, eventType);
            await TriggerFunction(CreateMessage(eventMsg));
        }

        public void ThenAnAssetNotIndexedExceptionIsThrown(string id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(AssetNotIndexedException));
            (_lastException as AssetNotIndexedException).Id.Should().Be(id);
        }

        public void ThenATenureNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<TenureInformation>));
            (_lastException as EntityNotFoundException<TenureInformation>).Id.Should().Be(id);
        }

        public async Task ThenTheTenureIndexIsUpdated(
            TenureInformation tenure, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableTenure>(tenure.Id, g => g.Index("tenures"))
                                       .ConfigureAwait(false);

            var tenureInIndex = result.Source;
            tenureInIndex.Should().BeEquivalentTo(_entityFactory.CreateQueryableTenure(tenure));
        }

        public async Task ThenTheAssetIndexIsUpdatedWithTheTenure(
            TenureInformation tenure, QueryableAsset asset, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableAsset>(tenure.TenuredAsset.Id, g => g.Index("assets"))
                                       .ConfigureAwait(false);

            var assetInIndex = result.Source;
            assetInIndex.Should().BeEquivalentTo(asset, c => c.Excluding(x => x.Tenure));
            assetInIndex.Tenure.EndOfTenureDate.Should().Be(tenure.EndOfTenureDate);
            assetInIndex.Tenure.Id.Should().Be(tenure.Id);
            assetInIndex.Tenure.PaymentReference.Should().Be(tenure.PaymentReference);
            assetInIndex.Tenure.StartOfTenureDate.Should().Be(tenure.StartOfTenureDate);
            assetInIndex.Tenure.Type.Should().Be(tenure.TenureType.Description);
        }

        public async Task ThenThePersonIndexIsUpdatedWithTheTenure(
            TenureInformation tenure, IElasticClient esClient)
        {
            foreach (var hm in tenure.HouseholdMembers)
            {
                var result = await esClient.GetAsync<QueryablePerson>(hm.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);
                var p = result.Source;
                var pt = p.Tenures.FirstOrDefault(x => x.Id == tenure.Id);
                pt.Should().NotBeNull();

                pt.AssetFullAddress.Should().Be(tenure.TenuredAsset.FullAddress);
                pt.EndDate.Should().Be(tenure.EndOfTenureDate);
                pt.Id.Should().Be(tenure.Id);
                pt.PaymentReference.Should().Be(tenure.PaymentReference);
                pt.StartDate.Should().Be(tenure.StartOfTenureDate);
                pt.Type.Should().Be(tenure.TenureType.Description);
            }
        }
    }
}
