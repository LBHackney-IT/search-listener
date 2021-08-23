<<<<<<< HEAD
﻿using System.Collections.Generic;

namespace HousingSearchListener.V1.Domain.Tenure
{
    public class TenureInformation
    {
        public string Id { get; set; }
        public TenuredAsset TenuredAsset { get; set; }
        public string StartOfTenureDate { get; set; }
        public string EndOfTenureDate { get; set; }
        public TenureType TenureType { get; set; }
        public bool IsActive { get; set; }
        public List<HouseholdMembers> HouseholdMembers { get; set; }
        public string PaymentReference { get; set; }
    }
}
