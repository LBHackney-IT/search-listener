using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Domain.Person;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class EsGateway : IEsGateway
    {
        private readonly IElasticClient _elasticClient;

        private const string IndexNamePersons = "persons";
        private const string IndexNameTenures = "tenures";

        public EsGateway(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        [LogCall]
        private async Task<IndexResponse> ESIndex<T>(T esObject, string indexName) where T : class
        {
            return await _elasticClient.IndexAsync(new IndexRequest<T>(esObject, indexName));
        }

        public async Task<IndexResponse> IndexPerson(ESPerson esPerson)
        {
            if (esPerson is null)
            {
                throw new ArgumentNullException(nameof(esPerson));
            }

            return await ESIndex(esPerson, IndexNamePersons);
        }

        public async Task<IndexResponse> IndexTenure(QueryableTenure esTenure)
        {
            if (esTenure is null)
            {
                throw new ArgumentNullException(nameof(esTenure));
            }

            return await ESIndex(esTenure, IndexNameTenures);
        }

        [LogCall]
        public async Task<UpdateResponse<Person>> UpdatePersonAsync(ESPerson esPerson, ESTenure tenure)
        {
            if (esPerson is null)
            {
                throw new ArgumentNullException(nameof(esPerson));
            }

            var esTenure = esPerson.Tenures.Where(t => t.Id.Equals(tenure.Id)).FirstOrDefault();
            if (esTenure is null)
            {
                throw new ArgumentException($"Tenure with id: {tenure.Id} does not exist!");
            }

            esTenure.TotalBalance = tenure.TotalBalance;

            return await _elasticClient.UpdateAsync<Person, object>(esPerson.Id, descriptor => descriptor
                .Index(IndexNamePersons)
                .Doc(new { totalBalance = esPerson.TotalBalance, tenures = esPerson.Tenures })
                .DocAsUpsert(true));
        }

        [LogCall]
        public async Task<UpdateResponse<Person>> AddTenureToPersonIndexAsync(ESPerson esPerson, ESTenure esTenure)
        {
            if (esPerson is null)
            {
                throw new ArgumentNullException(nameof(esPerson));
            }

            if (esPerson.Tenures.Any(t => t.Id.Equals(esTenure.Id)))
            {
                throw new ArgumentException($"Tenure with id: {esTenure.Id} already exist!");
            }

            esPerson.Tenures.Add(esTenure);

            return await _elasticClient.UpdateAsync<Person, object>(esPerson.Id, descriptor => descriptor
                .Index(IndexNamePersons)
                .Doc(new { totalBalance = esPerson.TotalBalance, tenures = esPerson.Tenures })
                .DocAsUpsert(true));
        }
    }
}