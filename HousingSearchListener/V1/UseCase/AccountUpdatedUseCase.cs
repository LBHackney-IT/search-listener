using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Accounts;
using Hackney.Shared.HousingSearch.Domain.Accounts.Enum;
using HousingSearchListener.V1.Factories.Interfaces;
using HousingSearchListener.V1.Gateway;
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
        private readonly IEsGateway _esGateway;
        private readonly IAccountDbGateway _accountApiGateway;
        private readonly IAccountFactory _accountFactory;

        public AccountUpdatedUseCase(IAccountDbGateway accountApiGateway, IAccountFactory accountFactory, IEsGateway esGateway)
        {
            _accountApiGateway = accountApiGateway;
            _accountFactory = accountFactory;
            _esGateway = esGateway;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            //Get details account of current Entity
            var entity = await _accountApiGateway.GetByIdAsync(message.EntityId).ConfigureAwait(false);

            if (entity is null)
            {
                throw new EntityNotFoundException<Account>(message.EntityId);
            }

            await HandleAccountBalanceCalculation(entity);
        }

        private async Task HandleAccountBalanceCalculation(Account entity)
        {
            var initialTotalBalance = entity.AccountBalance;
            var masterEntity = entity;

            //check if account type is not master 
            if (entity.AccountType != AccountType.Master)
            {
                masterEntity = await _accountApiGateway.GetByIdAsync(entity.ParentAccountId).ConfigureAwait(false);
                initialTotalBalance = masterEntity.AccountBalance;
            }
            var allSubAccounts = await _accountApiGateway.GetByParentIdAsync(masterEntity.Id).ConfigureAwait(false);

            if (allSubAccounts.Any())
            {
                initialTotalBalance += allSubAccounts.Sum(a => a.AccountBalance);
            }

            var esMasterAccount = _accountFactory.ToQueryableAccount(masterEntity);
            esMasterAccount.AccountBalance = initialTotalBalance;

            await _esGateway.IndexAccount(esMasterAccount);

            await _accountApiGateway.UpdateAccountBalance(esMasterAccount.Id, initialTotalBalance).ConfigureAwait(false);
        }
    }
}
