using HousingSearchListener.V1.Domain.Account;
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

        Task<UpdateResponse<Person>> UpdatePersonAccountAsync(ESPerson esPerson, ESPersonTenure tenure);

        Task<UpdateResponse<Person>> AddTenureToPersonIndexAsync(ESPerson esPerson, ESPersonTenure esTenure);

        Task<UpdateResponse<Person>> UpdatePersonBalanceAsync(ESPerson esPerson, Account account);
    }
}
