using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;

namespace HousingSearchListener
{
    public interface IElasticSearchUpdater
    {
        Task Update(SNSEvent snsEvent);
    }
}