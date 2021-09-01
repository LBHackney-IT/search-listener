using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Domain.Person;
using Nest;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public interface IEsGateway
    {
        Task<IndexResponse> IndexPerson(ESPerson esPerson);

        Task<IndexResponse> IndexTenure(QueryableTenure esTenure);

        Task<UpdateResponse<Person>> UpdatePersonAsync(ESPerson esPerson, ESTenure tenure);

        Task<UpdateResponse<Person>> AddTenureToPersonIndexAsync(ESPerson esPerson, ESTenure esTenure);
    }
}
