using System.Collections.Generic;

namespace HousingSearchListener.V1.Domain.Asset
{
    public class AssetLocation
    {
        public string FloorNo { get; set; }
        public int TotalBlockFloors { get; set; }
        public IEnumerable<ParentAsset> ParentAssets { get; set; }
    }
}
