using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using HousingSearchListener.V1.Domain;
using HousingSearchListener.V1.Domain.ElasticSearch;

namespace HousingSearchListener.V1.UseCase
{
    public interface IUpdatePersonUseCase
    {
        Task Update(ESPerson esPerson);
    }
}