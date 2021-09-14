using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class EsGateway : IEsGateway
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<EsGateway> _logger;

        private const string IndexNamePersons = "persons";
        private const string IndexNameTenures = "tenures";

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

        [LogCall]
        public async Task<IndexResponse> IndexPerson(QueryablePerson esPerson)
        {
            if (esPerson is null) throw new ArgumentNullException(nameof(esPerson));

            _logger.LogDebug($"Updating '{IndexNamePersons}' index for person id {esPerson.Id}");
            return await ESIndex(esPerson, IndexNamePersons);
        }

        [LogCall]
        public async Task<IndexResponse> IndexTenure(QueryableTenure esTenure)
        {
            if (esTenure is null) throw new ArgumentNullException(nameof(esTenure));

            _logger.LogDebug($"Updating '{IndexNameTenures}' index for tenure id {esTenure.Id}");
            return await ESIndex(esTenure, IndexNameTenures);
        }
    }
}