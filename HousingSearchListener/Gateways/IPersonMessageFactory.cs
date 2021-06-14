using Amazon.Lambda.SNSEvents;
using HousingSearchListener.V1.Domain;

namespace HousingSearchListener.Gateways
{
    public interface IPersonMessageFactory
    {
        PersonCreatedMessage Create(SNSEvent.SNSRecord record);
    }
}