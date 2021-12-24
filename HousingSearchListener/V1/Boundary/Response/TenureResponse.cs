using System.Collections.Generic;
using System.Linq;
using AccountTenure = Hackney.Shared.HousingSearch.Domain.Accounts.Tenure;
namespace HousingSearchListener.V1.Domain.Account
{
    public class TenureResponse
    {
        public string TenureId { get; set; }
        public TenureTypeResponse TenureType { get; set; }
        public List<PrimaryTenantsResponse> PrimaryTenants { get; set; }
        public string FullAddress { get; set; }

        public AccountTenure ToTenure()
        {
            return AccountTenure.Create(TenureId, 
                TenureType?.ToTenureType(), 
                FullAddress, 
                PrimaryTenants?.Select(_ => _.ToPrimaryTenant()).ToList());
        }
    }
}
