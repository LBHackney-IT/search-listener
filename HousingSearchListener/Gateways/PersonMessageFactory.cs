using Amazon.Lambda.SNSEvents;
using HousingSearchListener.V1.Domain;
using Newtonsoft.Json;

namespace HousingSearchListener.Gateways
{
    public class PersonMessageFactory : IPersonMessageFactory
    {
        public PersonCreatedMessage Create(SNSEvent.SNSRecord record)
        {
            return JsonConvert.DeserializeObject<PersonCreatedMessage>(record.Sns.Message);
        }
    }
}