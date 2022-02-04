using Hackney.Shared.HousingSearch.Domain.Accounts;
using System;

namespace HousingSearchListener.V1.Domain.Account
{
    public class PrimaryTenantsResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }

        public PrimaryTenant ToPrimaryTenant()
        {
            return PrimaryTenant.Create(Id, FullName);
        }
    }
}
