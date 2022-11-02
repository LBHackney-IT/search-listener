using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Domain.Contract;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class AddOrUpdateContractInAssetUseCase : IAddOrUpdateContractInAssetUseCase
    {
        private readonly ILogger<AddOrUpdateContractInAssetUseCase> _logger;
        private readonly IEsGateway _esGateway;
        private readonly IContractApiGateway _contractApiGateway;
        private readonly IAssetApiGateway _assetApiGateway;
        private readonly IESEntityFactory _esEntityFactory;

        public AddOrUpdateContractInAssetUseCase(IEsGateway esGateway, IContractApiGateway contractApiGateway,
            IAssetApiGateway assetApiGateway, IESEntityFactory esEntityFactory, ILogger<AddOrUpdateContractInAssetUseCase> logger)
        {
            _esGateway = esGateway;
            _contractApiGateway = contractApiGateway;
            _assetApiGateway = assetApiGateway;
            _esEntityFactory = esEntityFactory;
            _logger = logger;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Tenure from Tenure service API
            var contract = await _contractApiGateway.GetContractByIdAsync(message.EntityId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (contract is null) throw new EntityNotFoundException<Contract>(message.EntityId);

            // 2. Determine the Contract is for an Asset.
            if (!contract.TargetType.ToLower().Equals("asset"))
                throw new ArgumentException($"No charges of Types asset found for contract id: {contract.Id}");
            _logger.LogInformation($"Charges for contract {contract.Id} found. Now fetching Asset {contract.TargetId}");

            // 3. Get Added person from Person service API
            var assetId = Guid.Parse(contract.TargetId);
            var asset = await _assetApiGateway.GetAssetByIdAsync(assetId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (asset is null)
                throw new EntityNotFoundException<Contract>(assetId);


            //Remove all charges and re-add
            asset.Contract.Charges = null;

            foreach (var charge in contract.Charges)
            {
                var newCharge = new Charges();
                newCharge.Id = charge.Id;
                newCharge.Type = charge.Type;
                newCharge.SubType = charge.SubType;
                newCharge.Frequency = charge.Frequency;
                newCharge.Amount = charge.Amount;

                asset.Contract.Charges.ToList().Add(newCharge);
            }

            // 4. Update the indexes
            await UpdateAssetIndexAsync(asset);
        }

        private async Task UpdateAssetIndexAsync(Hackney.Shared.HousingSearch.Domain.Asset.Asset asset)
        {
            var esAsset = _esEntityFactory.CreateAsset(asset);
            await _esGateway.IndexAsset(esAsset);
        }
    }
}
