using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using Nest;
using System.Threading.Tasks;
using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;

namespace HousingSearchListener.V1.Gateway.Interfaces
{
    public interface IEsGateway
    {
        Task<IndexResponse> IndexPerson(QueryablePerson esPerson);

        Task<IndexResponse> IndexTenure(QueryableTenure esTenure);

        Task<IndexResponse> IndexAsset(QueryableAsset esAsset);

        Task<IndexResponse> IndexTransaction(QueryableTransaction esTransaction);

        Task<IndexResponse> IndexProcess(QueryableProcess esProcess);

        Task<QueryableAsset> GetAssetById(string id);

        Task<QueryableTenure> GetTenureById(string id);

        Task<QueryablePerson> GetPersonById(string id);
    }
}
