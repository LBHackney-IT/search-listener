using System;
using System.Text.Json.Serialization;
using Hackney.Shared.Tenure.Domain;

namespace HousingSearchListener.V1.Domain.Asset
{
    public class AssetTenure
    {
        public string Id { get; set; }
        public string PaymentReference { get; set; }
        public string Type { get; set; }
        public DateTime? StartOfTenureDate { get; set; }
        public DateTime? EndOfTenureDate { get; set; }
        [JsonIgnore]
        public bool IsActive => TenureHelpers.IsTenureActive(EndOfTenureDate);
    }
}
