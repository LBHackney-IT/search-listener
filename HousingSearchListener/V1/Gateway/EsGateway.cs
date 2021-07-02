using System.Threading.Tasks;
using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Interfaces;
using Nest;

namespace HousingSearchListener.V1.Gateway
{
    public class EsGateway : IEsGateway
    {
        private readonly IElasticClient _elasticClient;

        public EsGateway(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<IndexResponse> Update(ESPerson esPerson)
        {
            return await ESIndex(esPerson);
        }

        public async Task<IndexResponse> Create(ESPerson esPerson)
        {
            return await ESIndex(esPerson);
        }

        private async Task<IndexResponse> ESIndex(ESPerson esPerson)
        {
            return await _elasticClient.IndexAsync(new IndexRequest<ESPerson>(esPerson, "persons"));
        }
    }
}