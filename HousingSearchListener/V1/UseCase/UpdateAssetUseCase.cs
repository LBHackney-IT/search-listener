using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Gateway.Interfaces;
using Hackney.Shared.Asset.Domain;

namespace HousingSearchListener.V1.UseCase
{
    public class UpdateAssetUseCase : IUpdateAssetUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IAssetApiGateway _assetApiGateway;
        private readonly IESEntityFactory _esAssetFactory;

        public UpdateAssetUseCase(IEsGateway esGateway, IAssetApiGateway assetApiGateway,
            IESEntityFactory esAssetFactory)
        {
            _esGateway = esGateway;
            _assetApiGateway = assetApiGateway;
            _esAssetFactory = esAssetFactory;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Asset from Asset service API
            var asset = await _assetApiGateway.GetAssetByIdAsync(message.EntityId, message.CorrelationId)
                                         .ConfigureAwait(false);
            if (asset is null) throw new EntityNotFoundException<Asset>(message.EntityId);

            // 2. Update the ES index
            var esAsset = await _esGateway.GetAssetById(asset.Id.ToString()).ConfigureAwait(false);
            esAsset = _esAssetFactory.CreateAsset(asset);

            await _esGateway.IndexAsset(esAsset);
        }
    }
}