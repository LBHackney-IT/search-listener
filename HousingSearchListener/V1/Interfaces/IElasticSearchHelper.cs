using System.Threading.Tasks;
using HousingSearchListener.V1.Domain.ElasticSearch;
using Nest;

namespace HousingSearchListener.V1.Interfaces
{
    public interface IElasticSearchHelper
    {
        Task<CreateResponse> Create(ESPerson esPerson);
    }
}
