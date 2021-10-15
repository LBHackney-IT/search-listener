using Hackney.Core.Logging;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Domain.ElasticSearch.Asset;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class IndexTenureUseCase : IIndexTenureUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly ITenureApiGateway _tenureApiGateway;
        private readonly IESEntityFactory _esEntityFactory;

        public IndexTenureUseCase(IEsGateway esGateway, ITenureApiGateway tenureApiGateway,
            IESEntityFactory esPersonFactory)
        {
            _esGateway = esGateway;
            _tenureApiGateway = tenureApiGateway;
            _esEntityFactory = esPersonFactory;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Tenure from Tenure service API
            var tenure = await _tenureApiGateway.GetTenureByIdAsync(message.EntityId, message.CorrelationId)
                                         .ConfigureAwait(false);
            if (tenure is null) throw new EntityNotFoundException<TenureInformation>(message.EntityId);

            // 2. Get the asset for the tenure from the index
            // TODO - The asset info should really be retrieved directly from the Asset Api and then a QueryableAsset rebuilt
            // rather than just getting the current entry in the index.
            QueryableAsset queryableAsset = await _esGateway.GetAssetById(tenure.TenuredAsset.Id);
            if (queryableAsset is null) throw new AssetNotIndexedException(tenure.TenuredAsset.Id);

            // 3. Update the ES indexes
            var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);
            await _esGateway.IndexTenure(esTenure);
            await UpdateAssetForTenure(tenure, queryableAsset);
        }

        private async Task UpdateAssetForTenure(TenureInformation tenure, QueryableAsset queryableAsset)
        {
            queryableAsset.Tenure = _esEntityFactory.CreateAssetQueryableTenure(tenure);
            await _esGateway.IndexAsset(queryableAsset);
        }
    }
}