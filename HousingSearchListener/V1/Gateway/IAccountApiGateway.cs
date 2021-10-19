using HousingSearchListener.V1.Domain.Account;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public interface IAccountApiGateway
    {
        Task<AccountResponseObject> GetAccountByIdAsync(Guid id, Guid correlationId);
    }
}
