using System.Collections.Generic;

namespace HousingSearchListener.V1.Domain.Account
{
    public class Tenure
    {
        public string TenancyId { get; set; }

        public string TenancyType { get; set; }

        public string FullAddress { get; set; }

        public IEnumerable<PrimaryTenants> PrimaryTenants { get; set; }
    }
}
