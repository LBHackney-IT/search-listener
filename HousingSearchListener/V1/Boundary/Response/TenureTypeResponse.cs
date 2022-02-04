using Hackney.Shared.HousingSearch.Domain.Accounts;

namespace HousingSearchListener.V1.Domain.Account
{
    public class TenureTypeResponse
    {
        public string Code { get; set; }
        public string Description { get; set; }

        public TenureType ToTenureType()
        {
            return TenureType.Create(Code, Description);
        }
    }
}
