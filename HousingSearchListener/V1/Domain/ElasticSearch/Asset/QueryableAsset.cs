using Nest;

namespace HousingSearchListener.V1.Domain.ElasticSearch.Asset
{
    public class QueryableAsset
    {
        [Text(Name = "id")]
        public string Id { get; set; }

        [Text(Name = "assetId")]
        public string AssetId { get; set; }

        [Text(Name = "assetType")]
        public string AssetType { get; set; }

        [Text(Name = "isAssetCautionaryAlerted")]
        public bool IsAssetCautionaryAlerted { get; set; }

        public QueryableAssetAddress AssetAddress { get; set; }

        public QueryableTenure Tenure { get; set; }
    }
}
