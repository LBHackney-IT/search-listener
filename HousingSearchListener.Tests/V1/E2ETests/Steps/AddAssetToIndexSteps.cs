using FluentAssertions;
using Hackney.Shared.HousingSearch.Domain.Asset;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class AddAssetToIndexSteps : BaseSteps
    {
        private readonly ESEntityFactory _entityFactory = new ESEntityFactory();

        public AddAssetToIndexSteps()
        {
            _eventType = EventTypes.AssetCreatedEvent;
        }

        public async Task WhenTheFunctionIsTriggered(Guid AssetId, string eventType)
        {
            var eventMsg = CreateEvent(AssetId, eventType);
            await TriggerFunction(CreateMessage(eventMsg));
        }

        public void ThenAnAssetNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<Asset>));
            (_lastException as EntityNotFoundException<Asset>).Id.Should().Be(id);
        }

        public async Task ThenTheIndexIsUpdatedWithTheAsset(
            Asset Asset, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableAsset>(Asset.Id, g => g.Index("assets"))
                                       .ConfigureAwait(false);

            var AssetInIndex = result.Source;
            AssetInIndex.Should().BeEquivalentTo(_entityFactory.CreateAsset(Asset));
        }
    }
}
