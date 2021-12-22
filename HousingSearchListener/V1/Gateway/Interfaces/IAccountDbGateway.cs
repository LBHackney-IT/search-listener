using Hackney.Shared.HousingSearch.Domain.Accounts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway.Interfaces
{
    public interface IAccountDbGateway
    {
        /// <summary>
        /// Get account record by accountId
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>Account domain model or null</returns>
        Task<Account> GetByIdAsync(Guid accountId);

        /// <summary>
        /// Get a list of accounts by parentId
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns>A list of accountdomain models</returns>
        Task<List<Account>> GetByParentIdAsync(Guid parentId);

        /// <summary>
        /// Update account_balance field for account by accountId
        /// </summary>
        /// <param name="accountId">account id</param>
        /// <param name="balance">New balance value</param>
        /// <returns></returns>
        Task UpdateAccountBalance(Guid accountId, decimal balance);

        /// <summary>
        /// create account with tenure info
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task CreateAccount(Account account);
    }
}
