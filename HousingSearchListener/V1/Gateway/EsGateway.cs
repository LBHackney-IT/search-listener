using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.ElasticSearch;
using Nest;
using System;
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
            if (esPerson is null) throw new ArgumentNullException(nameof(esPerson));
            return await ESIndex(esPerson, IndexNamePersons);
        }

        public async Task<IndexResponse> IndexTenure(ESTenure esTenure)
        {
            if (esTenure is null) throw new ArgumentNullException(nameof(esTenure));
            return await ESIndex(esTenure, IndexNameTenures);
        }
    }
}