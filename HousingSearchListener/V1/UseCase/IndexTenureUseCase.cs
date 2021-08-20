using HousingSearchListener.V1.Boundary;
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
            if (tenure is null) throw new TenureNotFoundException(message.EntityId);

            // 2. Update the ES index
            var esTenure = _esEntityFactory.CreateTenure(tenure);
            await _esGateway.IndexTenure(esTenure);
        }
    }
}