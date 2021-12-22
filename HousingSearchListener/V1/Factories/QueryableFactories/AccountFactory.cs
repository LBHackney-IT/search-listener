using Hackney.Shared.HousingSearch.Domain.Accounts;
using Hackney.Shared.HousingSearch.Gateways.Models.Accounts;
using HousingSearchListener.V1.Factories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using QueryableAccountTenure = Hackney.Shared.HousingSearch.Gateways.Models.Accounts.QueryableTenure;

namespace HousingSearchListener.V1.Factories.QueryableFactories
{
    public class AccountFactory : IAccountFactory
    {
        public QueryableAccount ToQueryableAccount(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (account.Tenure == null)
                throw new Exception("There is no tenure provided for this account.");

            return new QueryableAccount
            {
                Id = account.Id,
                PaymentReference = account.PaymentReference,
                AccountBalance = account.AccountBalance,
                AccountStatus = account.AccountStatus,
                AccountType = account.AccountType,
                AgreementType = account.AgreementType,
                ConsolidatedBalance = account.ConsolidatedBalance,
                CreatedAt = account.CreatedAt,
                CreatedBy = account.CreatedBy,
                EndDate = account.EndDate,
                LastUpdatedAt = account.LastUpdatedAt,
                LastUpdatedBy = account.LastUpdatedBy,
                ParentAccountId = account.ParentAccountId,
                RentGroupType = account.RentGroupType,
                StartDate = account.StartDate,
                TargetId = account.TargetId,
                TargetType = account.TargetType,
                ConsolidatedCharges = (List<QueryableConsolidatedCharge>)(account.ConsolidatedCharges?.Select(p =>
                    new QueryableConsolidatedCharge
                    {
                        Amount = p.Amount,
                        Frequency = p.Frequency,
                        Type = p.Type
                    })),
                Tenure = new QueryableAccountTenure
                {
                    FullAddress = account.Tenure.FullAddress,
                    PrimaryTenants = account.Tenure?.PrimaryTenants.Select(s =>
                        new QueryablePrimaryTenant
                        {
                            Id = s.Id,
                            FullName = s.FullName
                        }).ToList()
                }
            };
        }
    }
}
