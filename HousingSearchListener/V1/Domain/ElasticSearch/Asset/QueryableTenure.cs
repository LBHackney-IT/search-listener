using Nest;

namespace HousingSearchListener.V1.Domain.ElasticSearch.Asset
{
    public class QueryableTenure
    {
        [Text(Name = "id")]
        public string Id { get; set; }
        public string PaymentReference { get; set; }
        public QueryableTenuredAsset TenuredAsset { get; set; }
        public string StartOfTenureDate { get; set; }
        public string EndOfTenureDate { get; set; }
        public string Type { get; set; }
    }
}
