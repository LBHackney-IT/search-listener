using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.ElasticSearch.Asset;
using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class EsGateway : IEsGateway
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<EsGateway> _logger;

        public const string IndexNamePersons = "persons";
        public const string IndexNameTenures = "tenures";
        public const string IndexNameAssets = "assets";

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
        public async Task<IndexResponse> IndexTenure(Domain.ElasticSearch.Tenure.QueryableTenure esTenure)
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
        public async Task<QueryableAsset> GetAssetById(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            return await GetById<QueryableAsset>(id, IndexNameAssets);
        }

        [LogCall]
        public async Task<UpdateResponse<QueryablePerson>> AddTenureToPersonAsync(QueryablePerson person, QueryablePersonTenure tenure)
        {
            if (person is null)
            {
                throw new ArgumentNullException(nameof(person));
            }

            if (tenure is null)
            {
                throw new ArgumentNullException(nameof(tenure));
            }

            var esPerson = await GetById<QueryablePerson>(person.Id, IndexNamePersons).ConfigureAwait(false);

            if (esPerson.Tenures.Any(t => t.Id.Equals(tenure.Id)))
            {
                throw new ArgumentException($"Tenure with id: {tenure.Id} already exist!");
            }

            esPerson.Tenures.Add(tenure);

            _logger.LogDebug($"Updating '{IndexNamePersons}' index for person id {esPerson.Id}");

            return await _elasticClient.UpdateAsync<QueryablePerson, object>(esPerson.Id, descriptor => descriptor
                .Index(IndexNamePersons)
                .Doc(new { tenures = esPerson.Tenures })
                .DocAsUpsert(true));
        }
    }
}