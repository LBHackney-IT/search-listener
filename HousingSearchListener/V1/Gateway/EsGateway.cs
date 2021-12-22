using Hackney.Core.Logging;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Threading.Tasks;
using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;

namespace HousingSearchListener.V1.Gateway
{
    public class EsGateway : IEsGateway
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<EsGateway> _logger;

        public const string IndexAccounts = "accounts";
        public const string IndexNamePersons = "persons";
        public const string IndexNameTenures = "tenures";
        public const string IndexNameAssets = "assets";
        public const string IndexNameTransactions = "transactions";

        public EsGateway(IElasticClient elasticClient, ILogger<EsGateway> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        private async Task<IndexResponse> ESIndex<T>(T esObject, string indexName) where T : class
        {
            _logger.LogDebug($"Updating {indexName}");
            return await _elasticClient.IndexAsync(new IndexRequest<T>(esObject, indexName));
        }

        private async Task<T> GetById<T>(string id, string indexName) where T : class
        {
            var getResponse = await _elasticClient.GetAsync<T>(new GetRequest<T>(indexName, id));
            return getResponse.Found ? getResponse.Source : null;
        }

        [LogCall]
        public async Task<IndexResponse> IndexPerson(QueryablePerson esPerson)
        {
            if (esPerson is null) throw new ArgumentNullException(nameof(esPerson));

            _logger.LogDebug($"Updating '{IndexNamePersons}' index for person id {esPerson.Id}");
            return await ESIndex(esPerson, IndexNamePersons);
        }
        [LogCall]
        public async Task<IndexResponse> IndexAccount(Hackney.Shared.HousingSearch.Gateways.Models.Accounts.QueryableAccount esAccount)
        {
            if (esAccount is null) throw new ArgumentNullException(nameof(esAccount));

            _logger.LogDebug($"Updating '{IndexNamePersons}' index for person id {esAccount.Id}");
            return await ESIndex(esAccount, IndexAccounts);
        }
        public async Task<IndexResponse> IndexTenure(Hackney.Shared.HousingSearch.Gateways.Models.Tenures.QueryableTenure esTenure)
        {
            if (esTenure is null) throw new ArgumentNullException(nameof(esTenure));

            _logger.LogDebug($"Updating '{IndexNameTenures}' index for tenure id {esTenure.Id}");
            return await ESIndex(esTenure, IndexNameTenures);
        }
        [LogCall]
        public async Task<IndexResponse> IndexAsset(QueryableAsset esAsset)
        {
            if (esAsset is null) throw new ArgumentNullException(nameof(esAsset));

            _logger.LogDebug($"Updating '{IndexNameAssets}' index for asset id {esAsset.Id}");
            return await ESIndex(esAsset, IndexNameAssets);
        }
        [LogCall]
        public async Task<IndexResponse> IndexTransaction(QueryableTransaction esTransaction)
        {
            if (esTransaction is null) throw new ArgumentNullException(nameof(esTransaction));

            _logger.LogDebug($"Updating '{IndexNameTransactions}' index for Transaction id {esTransaction.Id}");
            return await ESIndex(esTransaction, IndexNameTransactions);
        }

        [LogCall]
        public async Task<QueryableAsset> GetAssetById(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            return await GetById<QueryableAsset>(id, IndexNameAssets);
        }

        [LogCall]
        public async Task<QueryableTenure> GetTenureById(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            return await GetById<QueryableTenure>(id, IndexNameTenures);
        }

        [LogCall]
        public async Task<QueryablePerson> GetPersonById(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            return await GetById<QueryablePerson>(id, IndexNamePersons);
        }

        Task<Hackney.Shared.HousingSearch.Gateways.Models.Accounts.QueryableTenure> IEsGateway.GetTenureById(string id)
        {
            throw new NotImplementedException();
        }
    }
}