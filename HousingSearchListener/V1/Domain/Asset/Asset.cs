using System;

namespace HousingSearchListener.V1.Domain.Asset
{
    public class Asset
    {
        public Guid Id { get; set; }
        public string AssetId { get; set; }
        public AssetType AssetType { get; set; }
        public string RootAsset { get; set; }
        public string ParentAssetIds { get; set; }

        public AssetLocation AssetLocation { get; set; }
        public AssetAddress AssetAddress { get; set; }
        public AssetManagement AssetManagement { get; set; }
        public AssetCharacteristics AssetCharacteristics { get; set; }
        public AssetTenure Tenure { get; set; }
        public int? VersionNumber { get; set; }

        public static Asset Create(string id,
            string assetId,
            string assetType,
            AssetAddress assetAddress,
            AssetTenure tenure)
        {
            return new Asset
            {
                Id = Guid.Parse(id),
                AssetId = assetId,
                AssetType = (AssetType)Enum.Parse(typeof(AssetType), assetType),
                AssetAddress = assetAddress,
                Tenure = tenure
            };
        }
    }
}
