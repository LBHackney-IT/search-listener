using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using Nest;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public interface IEsGateway
    {
        Task<IndexResponse> IndexPerson(QueryablePerson esPerson);

        Task<IndexResponse> IndexTenure(QueryableTenure esTenure);
    }
}
