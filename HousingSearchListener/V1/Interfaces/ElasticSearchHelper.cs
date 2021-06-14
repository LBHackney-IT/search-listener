using System.Threading.Tasks;
using HousingSearchListener.V1.Domain.ElasticSearch;
using Nest;

namespace HousingSearchListener.V1.Interfaces
{
    public class ElasticSearchHelper : IElasticSearchHelper
    {
        private readonly IElasticClient _elasticClient;

        public ElasticSearchHelper(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<CreateResponse> Create(ESPerson esPerson)
        {
            return await _elasticClient.CreateAsync<ESPerson>(new CreateRequest<ESPerson>(esPerson));
        }
    }
}