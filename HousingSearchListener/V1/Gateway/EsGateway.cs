using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.Account;
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

        /// <summary>
        /// Update the account
        /// </summary>
        /// <param name="esPerson"></param>
        /// <param name="tenure"></param>
        /// <returns></returns>
        [LogCall]
        public async Task<UpdateResponse<Person>> UpdatePersonAccountAsync(ESPerson esPerson, ESPersonTenure tenure)
        {
            var esTenure = esPerson.Tenures.Where(t => t.Id.Equals(tenure.Id)).FirstOrDefault();
            if (esTenure is null)
            {
                throw new ArgumentException($"Tenure with id: {tenure.Id} does not exist!");
            }

            esTenure.TotalBalance = tenure.TotalBalance;

            return await _elasticClient.UpdateAsync<Person, object>(esPerson.Id, descriptor => descriptor
                .Index(IndexNamePersons)
                .Doc(new { tenures = esPerson.Tenures })
                .DocAsUpsert(true));
        }

        /// <summary>
        /// Add a new account
        /// </summary>
        /// <param name="esPerson"></param>
        /// <param name="esTenure"></param>
        /// <returns></returns>
        [LogCall]
        public async Task<UpdateResponse<Person>> AddTenureToPersonIndexAsync(ESPerson esPerson, ESPersonTenure esTenure)
        {
            if (esPerson.Tenures.Any(t => t.Id.Equals(esTenure.Id)))
            {
                throw new ArgumentException($"Tenure with id: {esTenure.Id} already exist!");
            }

            esPerson.Tenures.Add(esTenure);

            return await _elasticClient.UpdateAsync<Person, object>(esPerson.Id, descriptor => descriptor
                .Index(IndexNamePersons)
                .Doc(new { tenures = esPerson.Tenures })
                .DocAsUpsert(true));
        }

        /// <summary>
        /// Update the person balance
        /// </summary>
        /// <param name="esPerson"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        [LogCall]
        public async Task<UpdateResponse<Person>> UpdatePersonBalanceAsync(ESPerson esPerson, Account account)
        {
            return await _elasticClient.UpdateAsync<Person, object>(esPerson.Id, descriptor => descriptor
                .Index(IndexNamePersons)
                .Doc(new { totalBalance = account.AccountBalance })
                .DocAsUpsert(true));
        }
    }
}