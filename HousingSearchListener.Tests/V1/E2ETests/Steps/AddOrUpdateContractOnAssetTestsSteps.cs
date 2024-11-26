using FluentAssertions;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Nest;
using System;
using System.Threading.Tasks;
using Hackney.Shared.HousingSearch.Domain.Contract;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;
using System.Linq;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class AddOrUpdateContractOnAssetTestsSteps : BaseSteps
    {
        public AddOrUpdateContractOnAssetTestsSteps()
        {
            _eventType = EventTypes.ContractCreatedEvent;
        }

        public async Task WhenTheFunctionIsTriggered(Guid contractId, string eventType, string targetType)
        {
            var eventMsg = CreateEvent(contractId, eventType, targetType);
            await TriggerFunction(CreateMessage(eventMsg));
        }

        public void ThenAContractNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<Contract>));
            (_lastException as EntityNotFoundException<Contract>).Id.Should().Be(id);
        }

        public void ThenAnAssetNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<QueryableAsset>));
            (_lastException as EntityNotFoundException<QueryableAsset>).Id.Should().Be(id);
        }

        public async Task ThenTheAssetInTheIndexIsUpdatedWithTheContract(
            QueryableAsset asset, Contract contract, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableAsset>(asset.Id, g => g.Index("assets"))
                                       .ConfigureAwait(false);

            var assetInIndex = result.Source;
            assetInIndex.AssetContracts.First().Should().BeEquivalentTo(contract);
        }
    }
}