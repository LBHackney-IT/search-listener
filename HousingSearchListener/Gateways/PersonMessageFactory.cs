using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using HousingSearchListener.V1.Domain;
using Newtonsoft.Json;

namespace HousingSearchListener.Gateways
{
    public class PersonMessageFactory : IPersonMessageFactory
    {
        public PersonCreatedMessage Create(SQSEvent.SQSMessage record)
        {
            return JsonConvert.DeserializeObject<PersonCreatedMessage>(record.Body);
        }
    }
}