using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Accounts;
using HousingSearchListener.V1.Factories.Interfaces;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class AccountCreatedUseCase : IAccountCreateUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IAccountDbGateway _accountApiGateway;
        private readonly IAccountFactory _accountFactory;

        public AccountCreatedUseCase(IEsGateway esGateway, IAccountDbGateway accountApiGateway, IAccountFactory accountFactory)
        {
            _esGateway = esGateway;
            _accountApiGateway = accountApiGateway;
            _accountFactory = accountFactory;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var account = await _accountApiGateway.GetByIdAsync(message.EntityId)
                .ConfigureAwait(false);

            if (account == null)
                throw new EntityNotFoundException<Account>(message.EntityId);

            var esAccount = _accountFactory.ToQueryableAccount(account);
            await _esGateway.IndexAccount(esAccount);
        }
    }
}
