using Hackney.Shared.HousingSearch.Domain.Accounts;
using HousingSearchListener.V1.Domain.Account;
using System.Linq;

namespace HousingSearchListener.V1.Factories
{
    public static class EntityFactory
    {
        public static Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.AccountDbEntity AccountToDatabase(this Account account)
        {
            return account == null ? null : new Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.AccountDbEntity
            {
                Id = account.Id,
                AccountBalance = account.AccountBalance,
                AccountStatus = account.AccountStatus,
                EndDate = account.EndDate,
                CreatedBy = account.CreatedBy,
                CreatedAt = account.CreatedAt,
                LastUpdatedBy = account.LastUpdatedBy,
                LastUpdatedAt = account.LastUpdatedAt,
                StartDate = account.StartDate,
                TargetId = account.TargetId,
                TargetType = account.TargetType,
                AccountType = account.AccountType,
                AgreementType = account.AgreementType,
                RentGroupType = account.RentGroupType,
                ConsolidatedCharges = account.ConsolidatedCharges.Select(c => ConsolidatedChargeToDatabase(c)),
                Tenure = TenureToDatabase(account.Tenure),
                PaymentReference = account.PaymentReference,
                ParentAccountId = account.ParentAccountId
            };
        }

        public static Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.ConsolidatedChargeDbEntity ConsolidatedChargeToDatabase(this Hackney.Shared.HousingSearch.Domain.Accounts.ConsolidatedCharge consolidatedCharge)
        {
            return consolidatedCharge == null ? null : new Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.ConsolidatedChargeDbEntity
            {
                Amount = consolidatedCharge.Amount,
                Frequency = consolidatedCharge.Frequency,
                Type = consolidatedCharge.Type
            };
        }

        public static Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.TenureDbEntity TenureToDatabase(this Tenure tenure)
        {
            return tenure == null ? null : new Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.TenureDbEntity
            {
                FullAddress = tenure.FullAddress,
                PrimaryTenants = tenure.PrimaryTenants.Select(t => PrimaryTenantToDatabase(t)).ToList(),
                TenureType = TenureTypeToDatabase(tenure.TenureType),
                TenureId = tenure.TenureId
            };
        }

        public static Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.PrimaryTenantsDbEntity PrimaryTenantToDatabase(this Hackney.Shared.HousingSearch.Domain.Accounts.PrimaryTenant tenant)
        {
            return tenant == null ? null : new Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.PrimaryTenantsDbEntity
            {
                Id = tenant.Id,
                FullName = tenant.FullName,
            };
        }

        public static Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.TenureTypeDbEntity TenureTypeToDatabase(this TenureType tenureType)
        {
            return tenureType == null ? null : new Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.TenureTypeDbEntity
            {
                Code = tenureType.Code,
                Description = tenureType.Description
            };
        }


    }
}
