using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.Processes.Sns;
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
using HousingSearchApi.V1.Factories;
using Hackney.Shared.HousingSearch.Domain.Asset;

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

            /* 1. Get Contract from Contract service API
            var contract = await _contractApiGateway.GetContractByIdAsync(message.EntityId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (contract is null) throw new EntityNotFoundException<Contract>(message.EntityId);

            // 2. Determine the Contract is for an Asset.
            if (!contract.TargetType.ToLower().Equals("asset"))
                throw new ArgumentException($"No charges of Types asset found for contract id: {contract.Id}");
            _logger.LogInformation($"Contract with id {contract.Id} found. Now fetching Asset {contract.TargetId}");
            */

            //New process to handle multiple contracts
            //1. Get Asset data from message
            var assetMessageData = GetAssetDataFromEventData(message.EventData.NewData);

            var assetId = Guid.Parse(assetMessageData.Id);

            // 2. Get asset from Asset API

            var asset = await _assetApiGateway.GetAssetByIdAsync(assetId, message.CorrelationId)
                                                .ConfigureAwait(false) ?? throw new EntityNotFoundException<QueryableAsset>(assetId);
                                                
            var call = await _contractApiGateway.GetContractByIdAsync(assetId, message.CorrelationId).ConfigureAwait(false);

            // 3. Get all contracts from Contract API
            var allContracts = await _contractApiGateway.GetContractsByAssetIdAsync(assetId, message.CorrelationId).ConfigureAwait(false);

            // 4. Cycle over them to retrieve data (will need filters)
            if (allContracts.Results.Any())
            {
                _logger.LogInformation($"{allContracts.Results.Count()} contracts found.");

                var assetContracts = new List<QueryableAssetContract>();
                foreach (var assetContract in allContracts.Results)
                {
                    var queryableAssetContract = new QueryableAssetContract
                    {
                        Id = assetContract.Id,
                        TargetId = assetContract.TargetId,
                        TargetType = assetContract.TargetType,
                        EndDate = assetContract.EndDate,
                        EndReason = assetContract.EndReason,
                        ApprovalStatus = assetContract.ApprovalStatus,
                        ApprovalStatusReason = assetContract.ApprovalStatusReason,
                        IsActive = assetContract.IsActive,
                        ApprovalDate = assetContract.ApprovalDate,
                        StartDate = assetContract.StartDate
                    };

                    if (assetContract.Charges.Any())
                    {
                        _logger.LogInformation($"{assetContract.Charges.Count()} charges found.");
                        var charges = new List<QueryableCharges>();

                        foreach (var charge in assetContract.Charges)
                        {
                            _logger.LogInformation($"Charge with id {charge.Id} being added to asset with frequency {charge.Frequency}");
                            var queryableCharge = new QueryableCharges
                            {
                                Id = charge.Id,
                                Type = charge.Type,
                                SubType = charge.SubType,
                                Frequency = charge.Frequency,
                                Amount = charge.Amount
                            };
                            charges.Add(queryableCharge);
                        }

                        assetContract.Charges = charges;
                    }

                    if (assetContract.RelatedPeople.Any())
                    {
                        _logger.LogInformation($"{assetContract.RelatedPeople.Count()} related people found.");
                        var relatedPeople = new List<QueryableRelatedPeople>();

                        foreach (var relatedPerson in assetContract.RelatedPeople)
                        {
                            _logger.LogInformation($"Related person with id {relatedPerson.Id} being added to asset");
                            var queryableRelatedPeople = new QueryableRelatedPeople
                            {
                                Id = relatedPerson.Id,
                                Type = relatedPerson.Type,
                                SubType = relatedPerson.SubType,
                                Name = relatedPerson.Name,
                            };
                            relatedPeople.Add(queryableRelatedPeople);
                        }

                        assetContract.RelatedPeople = relatedPeople;
                    }
                    assetContracts.Add(queryableAssetContract);
                }
                asset.AssetContracts = assetContracts;
                // 5. Update the indexes
                await UpdateAssetIndexAsync(asset);
            }
        }
        private async Task UpdateAssetIndexAsync(QueryableAsset asset)
        {
            var esAsset = await _esGateway.GetAssetById(asset.Id.ToString()).ConfigureAwait(false);
            if (esAsset is null)
                throw new ArgumentException($"No asset found in index with id: {asset.Id}");
            esAsset = _esEntityFactory.CreateAsset(asset);
            await _esGateway.IndexAsset(esAsset);
        }
        private static Asset GetAssetDataFromEventData(object data)
        {
            return (data is Asset) ? data as Asset : ObjectFactory.ConvertFromObject<Asset>(data);
        }
    }
}