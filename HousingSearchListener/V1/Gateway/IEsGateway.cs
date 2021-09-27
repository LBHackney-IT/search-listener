using HousingSearchListener.V1.Domain.ElasticSearch.Asset;
using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Domain.Person;
using Nest;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public interface IEsGateway
    {
        Task<IndexResponse> IndexPerson(QueryablePerson esPerson);

        Task<IndexResponse> IndexTenure(Domain.ElasticSearch.Tenure.QueryableTenure esTenure);

        Task<IndexResponse> IndexAsset(QueryableAsset esAsset);

        Task<QueryableAsset> GetAssetById(string id);

        Task<UpdateResponse<QueryablePerson>> AddTenureToPersonAsync(QueryablePerson esPerson, QueryablePersonTenure esTenure);
    }
}
