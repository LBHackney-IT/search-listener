using Hackney.Shared.HousingSearch.Domain.Transactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;

namespace HousingSearchListener.V1.Infrastructure.Converters
{
    public class StringToTransactionTypeConverter : StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!objectType.IsEnum)
            {
                throw new ArgumentException($"From {nameof(StringToTransactionTypeConverter)}: provided type is not Enum!");
            }
            if (!objectType.Equals(typeof(TransactionType)))
            {
                throw new ArgumentException($"From {nameof(StringToTransactionTypeConverter)}: provided type is a TransactionType enum!");
            }

            var description = reader.Value.ToString();

            foreach (var field in objectType.GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description?.ToLower() == description?.ToLower())
                        return (TransactionType)field.GetValue(null);
                }
                else
                {
                    if (field.Name?.ToLower() == description?.ToLower())
                        return (TransactionType)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
            // Or return default(T);
        }
    }
}
