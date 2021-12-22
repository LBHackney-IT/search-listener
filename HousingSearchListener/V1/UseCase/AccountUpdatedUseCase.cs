using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Accounts;
using Hackney.Shared.HousingSearch.Domain.Accounts.Enum;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class AccountUpdatedUseCase : IAccountUpdateUseCase
    {
        private readonly IAccountDbGateway _gateway;

        public AccountUpdatedUseCase(IAccountDbGateway gateway)
        {
            _gateway = gateway;
        }
        public async Task ProcessMessageAsync(AccountSnsModel message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            //Get details account of current Entity
            var entity = await _gateway.GetByIdAsync(message.EntityId).ConfigureAwait(false);

            if (entity is null)
            {
                throw new EntityNotFoundException<Account>(message.EntityId);
            }

            await HandleAccountBalanceCalculation(entity);
        }

        public Task ProcessMessageAsync(EntityEventSns message)
        {
            throw new NotImplementedException();
        }

        private async Task HandleAccountBalanceCalculation(Account entity)
        {
            var totalBalance = entity.AccountBalance;
            var masterEntityToGetId = entity.Id;
            //check if account type is not master 
            if (entity.AccountType != AccountType.Master)
            {
                var masterEntity = await _gateway.GetByIdAsync(entity.ParentAccountId).ConfigureAwait(false);
                totalBalance = masterEntity.AccountBalance;
                masterEntityToGetId = masterEntity.Id;
                //entity.Id = masterEntity.Id;
            }
            var allSubAccounts = await _gateway.GetByParentIdAsync(masterEntityToGetId).ConfigureAwait(false);

            if (allSubAccounts.Any())
            {
                totalBalance += allSubAccounts.Sum(a => a.AccountBalance);
            }
            await _gateway.UpdateAccountBalance(masterEntityToGetId, totalBalance).ConfigureAwait(false);
        }
    }
}
