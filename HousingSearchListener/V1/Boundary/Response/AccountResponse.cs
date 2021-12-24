using Hackney.Shared.HousingSearch.Domain.Accounts;
using Hackney.Shared.HousingSearch.Domain.Accounts.Enum;
using HousingSearchListener.V1.Domain.Account;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HousingSearchListener.V1.Boundary.Response
{
    public class AccountResponse
    {
        public Guid Id { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal ConsolidatedBalance { get; set; }
        public List<ConsolidatedChargeResponse> ConsolidatedCharges { get; set; }
        public Guid ParentAccountId { get; set; }
        public string PaymentReference { get; set; }
        public string EndReasonCode { get; set; }
        public TargetType TargetType { get; set; }
        public Guid TargetId { get; set; }
        public AccountType AccountType { get; set; }
        public RentGroupType RentGroupType { get; set; }
        public string AgreementType { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public TenureResponse Tenure { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        public Account ToDomain()
        {
            return Account.Create(Id,
               ParentAccountId,
               PaymentReference,
               TargetType,
               TargetId,
               AccountType,
               RentGroupType,
               AgreementType,
               AccountBalance,
               ConsolidatedBalance,
               CreatedBy ?? string.Empty,
               LastUpdatedBy ?? string.Empty,
               CreatedAt,
               LastUpdatedAt,
               StartDate,
               EndDate,
               EndReasonCode,
               AccountStatus,
               ConsolidatedCharges?.Select(_ => _.ToConsolidatedCharge()).ToList(),
               Tenure?.ToTenure());
        }
    }
}
