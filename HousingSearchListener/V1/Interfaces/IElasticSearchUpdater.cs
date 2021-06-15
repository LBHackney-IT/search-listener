using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;

namespace HousingSearchListener.V1.Interfaces
{
    public interface IElasticSearchUpdater
    {
        Task Update(SQSEvent sqsEvent);
    }
}