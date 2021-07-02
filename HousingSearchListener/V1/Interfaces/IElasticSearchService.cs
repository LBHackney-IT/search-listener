using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;

namespace HousingSearchListener.V1.Interfaces
{
    public interface IElasticSearchService
    {
        Task Process(SQSEvent sqsEvent);
    }
}