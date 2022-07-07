using System;

namespace HousingSearchListener.V1.Domain.Asset
{
    public class AssetCharacteristics
    {
        public int NumberOfBedrooms { get; set; }
        public int NumberOfLifts { get; set; }
        public int NumberOfLivingRooms { get; set; }
        public string WindowType { get; set; }
        public string YearConstructed { get; set; }
        public int NumberOfBedSpaces { get; set; }
        public int NumberOfCots { get; set; }
        public int NumberOfShowerRooms { get; set; }
        public int NumberOfBathrooms { get; set; }
        public string AssetPropertyFolderLink { get; set; }
        public DateTime? EpcExpiryDate { get; set; }
        public DateTime? FireSafetyCertificateExpiryDate { get; set; }
        public DateTime? GasSafetyCertificateExpiryDate { get; set; }
        public DateTime? ElecCertificateExpiryDate { get; set; }


    }
}

