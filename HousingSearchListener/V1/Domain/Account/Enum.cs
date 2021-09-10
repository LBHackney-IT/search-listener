using System.Text.Json.Serialization;

namespace HousingSearchListener.V1.Domain.Account
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TargetType
    {
        Housing, Garage
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AccountStatus
    {
        Active, Suspended, Ended
    }
}
