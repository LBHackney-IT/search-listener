using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using Nest;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public interface IEsGateway
    {
        Task<IndexResponse> IndexPerson(QueryablePerson esPerson);

        Task<IndexResponse> IndexTenure(QueryableTenure esTenure);

        Task<IndexResponse> IndexAsset(QueryableAsset esAsset);

        Task<QueryableAsset> GetAssetById(string id);

        Task<QueryableTenure> GetTenureById(string id);

        Task<QueryablePerson> GetPersonById(string id);
    }
}
