using HousingSearchListener.V1.Domain.Account.Enums;
using System;

namespace HousingSearchListener.V1.Domain.Account
{
    public class Account
    {
        public Guid Id { get; set; }
        public string LastUpdatedBy { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal ConsolidatedBalance { get; set; }
        public Guid ParentAccountId { get; set; }
        public string PaymentReference { get; set; }
        public TargetType TargetType { get; set; }
        public Guid TargetId { get; set; }
        public AccountType AccountType { get; set; }
        public RentGroupType RentGroupType { get; set; }
        public string AgreementType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Tenure Tenure { get; set; }
    }
}
