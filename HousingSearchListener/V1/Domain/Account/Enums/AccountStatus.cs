using System.Text.Json.Serialization;

namespace HousingSearchListener.V1.Domain.Account.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AccountStatus
    {
        Active, 
        Suspended, 
        Ended
    }
}
