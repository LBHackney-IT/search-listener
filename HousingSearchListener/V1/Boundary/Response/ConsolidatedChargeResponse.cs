using Hackney.Shared.HousingSearch.Domain.Accounts;
using System;

namespace HousingSearchListener.V1.Domain.Account
{
    public class ConsolidatedChargeResponse
    {
        public string Type { get; set; }
        public string Frequency { get; set; }
        public Decimal Amount { get; set; }

        public ConsolidatedCharge ToConsolidatedCharge()
        {
            return ConsolidatedCharge.Create(Type, Frequency, Amount);
        }
    }
}
