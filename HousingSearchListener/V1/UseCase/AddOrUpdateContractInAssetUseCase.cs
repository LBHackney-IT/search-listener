using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Contract;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Contract;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

            // 1. Get Contract from Contract service API
            var contract = await _contractApiGateway.GetContractByIdAsync(message.EntityId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (contract is null) throw new EntityNotFoundException<Contract>(message.EntityId);

            // 2. Determine the Contract is for an Asset.
            if (!contract.TargetType.ToLower().Equals("asset"))
                throw new ArgumentException($"No charges of Types asset found for contract id: {contract.Id}");
            _logger.LogInformation($"Contract with id {contract.Id} found. Now fetching Asset {contract.TargetId}");

            // 3. Get Added person from Person service API
            var assetId = Guid.Parse(contract.TargetId);
            var asset = await _assetApiGateway.GetAssetByIdAsync(assetId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (asset is null)
                throw new EntityNotFoundException<Contract>(assetId);

            asset.AssetContract = new QueryableAssetContract()
            {
                Id = contract.Id,
            };

            if (contract.Charges.Any())
            {
                _logger.LogInformation($"{contract.Charges.Count()} charges found.");
                var charges = new List<QueryableCharges>();

                foreach (var charge in contract.Charges)
                {
                    _logger.LogInformation($"Charge with id {charge.Id} being added to asset with frequency {charge.Frequency}");
                    var queryableCharge = new QueryableCharges();
                    queryableCharge.Id = charge.Id;
                    queryableCharge.Type = charge.Type;
                    queryableCharge.SubType = charge.SubType;
                    queryableCharge.Frequency = charge.Frequency;
                    queryableCharge.Amount = charge.Amount;
                    charges.Add(queryableCharge);
                }

                asset.AssetContract.Charges = charges;
            }

            // 4. Update the indexes
            await UpdateAssetIndexAsync(asset);
        }

        private async Task UpdateAssetIndexAsync(QueryableAsset asset)
        {
            var esAsset = await _esGateway.GetAssetById(asset.Id.ToString()).ConfigureAwait(false);
            if (esAsset is null)
                throw new ArgumentException($"No asset found in index with id: {asset.Id}");
            esAsset = _esEntityFactory.CreateAsset(asset);
            await _esGateway.IndexAsset(esAsset);
        }
    }
}
