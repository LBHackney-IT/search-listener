using Hackney.Shared.HousingSearch.Domain.Accounts;
using Hackney.Shared.HousingSearch.Gateways.Models.Accounts;

namespace HousingSearchListener.V1.Factories.Interfaces
{
    public interface IAccountFactory
    {
        QueryableAccount ToQueryableAccount(Account account);
    }
}
