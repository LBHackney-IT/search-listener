using System.Text.Json;
using Hackney.Core.Http;

namespace HousingSearchApi.V1.Factories
{
    public static class ObjectFactory
    {
        public static T ConvertFromObject<T>(object obj) where T : class
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj), JsonOptions.Create());
        }
    }
}