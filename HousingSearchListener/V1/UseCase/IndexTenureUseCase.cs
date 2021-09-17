using HousingSearchListener.V1.Boundary;
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

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Tenure from Tenure service API
            var tenure = await _tenureApiGateway.GetTenureByIdAsync(message.EntityId)
                                         .ConfigureAwait(false);
            if (tenure is null) throw new EntityNotFoundException<TenureInformation>(message.EntityId);

            // 2. Get the asset for the tenure from the index if needed
            QueryableAsset queryableAsset = await _esGateway.GetAssetById(tenure.TenuredAsset.Id);
            if (queryableAsset is null) throw new AssetNotIndexedException(tenure.TenuredAsset.Id);

            // 3. Update the ES index
            var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);
            await _esGateway.IndexTenure(esTenure);

            await UpdateAssetForTenure(tenure, queryableAsset);
        }

        private async Task UpdateAssetForTenure(TenureInformation tenure, QueryableAsset queryableAsset)
        {
            if (queryableAsset.Tenure is null || queryableAsset.Tenure.Id != tenure.Id)
                queryableAsset.Tenure = new Domain.ElasticSearch.Asset.QueryableTenure() { Id = tenure.Id };

            queryableAsset.Tenure.EndOfTenureDate = tenure.EndOfTenureDate;
            queryableAsset.Tenure.PaymentReference = tenure.PaymentReference;
            queryableAsset.Tenure.StartOfTenureDate = tenure.StartOfTenureDate;
            queryableAsset.Tenure.TenuredAsset = new Domain.ElasticSearch.Asset.QueryableTenuredAsset()
            {
                FullAddress = tenure.TenuredAsset.FullAddress,
                Id = tenure.TenuredAsset.Id,
                Type = tenure.TenuredAsset.Type,
                Uprn = tenure.TenuredAsset.Uprn,
            };
            queryableAsset.Tenure.Type = tenure.TenureType.Description;

            await _esGateway.IndexAsset(queryableAsset);
        }
    }
}