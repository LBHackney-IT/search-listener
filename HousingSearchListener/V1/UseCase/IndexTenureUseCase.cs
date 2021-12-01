using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // 3. Get all the person records for the tenure and update the tenure details
            var persons = await GetPersonsForTenure(tenure);
            foreach (var p in persons)
                UpdatePersonTenure(p, tenure);

            // 4. Update the ES indexes
            var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);
            await _esGateway.IndexTenure(esTenure);
            await UpdateAssetForTenure(tenure, queryableAsset);
            await UpdatePersonsForTenure(persons);
        }

        private void UpdatePersonTenure(QueryablePerson person, TenureInformation tenure)
        {
            var personTenure = person.Tenures.FirstOrDefault(x => x.Id == tenure.Id);
            if (personTenure is null)
            {
                personTenure = new QueryablePersonTenure();
                person.Tenures.Add(personTenure);
            }
            personTenure.AssetFullAddress = tenure.TenuredAsset.FullAddress;
            personTenure.EndDate = tenure.EndOfTenureDate;
            personTenure.PaymentReference = tenure.PaymentReference;
            personTenure.StartDate = tenure.StartOfTenureDate;
            personTenure.Type = tenure.TenureType.Description;
        }

        private async Task<List<QueryablePerson>> GetPersonsForTenure(TenureInformation tenure)
        {
            var persons = new List<QueryablePerson>();
            foreach (var hm in tenure.HouseholdMembers)
            {
                var p = await _esGateway.GetPersonById(hm.Id).ConfigureAwait(false);
                if (p is null)
                    throw new EntityNotFoundException<QueryablePerson>(Guid.Parse(hm.Id));

                persons.Add(p);
            }
            return persons;
        }

        private async Task UpdateAssetForTenure(TenureInformation tenure, QueryableAsset queryableAsset)
        {
            queryableAsset.Tenure = _esEntityFactory.CreateAssetQueryableTenure(tenure);
            await _esGateway.IndexAsset(queryableAsset);
        }

        private async Task UpdatePersonsForTenure(List<QueryablePerson> persons)
        {
            foreach (var qp in persons)
                await _esGateway.IndexPerson(qp);
        }
    }
}