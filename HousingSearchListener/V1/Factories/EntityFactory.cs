using HousingSearchListener.V1.Domain.Account;
using HousingSearchListener.V1.Infrastructure;
using System.Linq;

namespace HousingSearchListener.V1.Factories
{
    public static class EntityFactory
    {
        public static Account ToDomain(this AccountDbEntity databaseEntity)
        {
            return databaseEntity == null ? null : new Account
            {
                Id = databaseEntity.Id,
                ParentAccountId = databaseEntity.ParentAccountId,
                PaymentReference = databaseEntity.PaymentReference,
                AccountBalance = databaseEntity.AccountBalance,
                ConsolidatedBalance = databaseEntity.ConsolidatedBalance,
                AccountStatus = databaseEntity.AccountStatus,
                EndDate = databaseEntity.EndDate,
                CreatedBy = databaseEntity.CreatedBy,
                CreatedAt = databaseEntity.CreatedAt,
                LastUpdatedBy = databaseEntity.LastUpdatedBy,
                LastUpdatedAt = databaseEntity.LastUpdatedAt,
                StartDate = databaseEntity.StartDate,
                TargetId = databaseEntity.TargetId,
                TargetType = databaseEntity.TargetType,
                AccountType = databaseEntity.AccountType,
                AgreementType = databaseEntity.AgreementType,
                RentGroupType = databaseEntity.RentGroupType,
                ConsolidatedCharges = databaseEntity.ConsolidatedCharges,
                Tenure = databaseEntity.Tenure
            };
        }
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

        public static Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.ConsolidatedChargeDbEntity ConsolidatedChargeToDatabase(this ConsolidatedCharge consolidatedCharge)
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

        public static Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.PrimaryTenantsDbEntity PrimaryTenantToDatabase(this PrimaryTenant tenant)
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
