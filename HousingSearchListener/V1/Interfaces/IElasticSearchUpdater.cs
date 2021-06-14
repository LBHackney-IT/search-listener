using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;

namespace HousingSearchListener.V1.Interfaces
{
    public interface IElasticSearchUpdater
    {
        Task Update(SNSEvent snsEvent);
    }
}