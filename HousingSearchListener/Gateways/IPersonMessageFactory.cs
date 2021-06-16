using Amazon.Lambda.SQSEvents;
using HousingSearchListener.V1.Domain;

namespace HousingSearchListener.Gateways
{
    public interface IPersonMessageFactory
    {
        PersonCreatedMessage Create(SQSEvent.SQSMessage record);
    }
}