using Nest;

namespace HousingSearchListener.V1.Domain.ElasticSearch.Asset
{
    public class QueryableTenuredAsset
    {
        [Text(Name = "fullAddress")]
        public string FullAddress { get; set; }
        public string Uprn { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
    }
}
