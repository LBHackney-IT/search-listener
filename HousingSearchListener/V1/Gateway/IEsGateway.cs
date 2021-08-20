using HousingSearchListener.V1.Domain.ElasticSearch;
using Nest;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public interface IEsGateway
    {
        Task<IndexResponse> IndexPerson(ESPerson esPerson);

        Task<IndexResponse> IndexTenure(ESTenure esTenure);
    }
}
