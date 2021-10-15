using System.Collections.Generic;

namespace HousingSearchListener.V1.Domain.Account
{
    public class AccountTenure
    {
        public string TenureId { get; set; }
        public string TenureType { get; set; }
        public List<AccountTenant> PrimaryTenants { get; set; }
        public string FullAddress { get; set; }
    }
}
