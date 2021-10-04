using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Domain.Account;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class AccountAddUseCase : IAccountAddUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly ITenureApiGateway _tenureApiGateway;
        private readonly IPersonApiGateway _personApiGateway;
        private readonly IAccountApiGateway _accountApiGateway;
        private readonly IESEntityFactory _esEntityFactory;

        public AccountAddUseCase(IEsGateway esGateway,
            ITenureApiGateway tenureApiGateway,
            IPersonApiGateway personApiGateway,
            IESEntityFactory esPersonFactory,
            IAccountApiGateway accountApiGateway)
        {
            _esGateway = esGateway;
            _tenureApiGateway = tenureApiGateway;
            _personApiGateway = personApiGateway;
            _esEntityFactory = esPersonFactory;
            _accountApiGateway = accountApiGateway;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var account = await _accountApiGateway.GetAccountByIdAsync(message.EntityId, message.CorrelationId).ConfigureAwait(false);
            if (account is null)
            {
                throw new EntityNotFoundException<Account>(message.EntityId);
            }

            var tenure = await _tenureApiGateway.GetTenureByIdAsync(account.TargetId, message.CorrelationId).ConfigureAwait(false);
            if (tenure is null)
            {
                throw new EntityNotFoundException<TenureInformation>(account.TargetId);
            }
            tenure.TotalBalance = account.AccountBalance;

            if (account.Tenure?.PrimaryTenants == null || account.Tenure.PrimaryTenants.Count() == 0)
            {
                throw new MissedEntityDataException($"Tenure with id {message.EntityId} does not have any responsible household members.");
            }

            foreach (var responsibleTenant in account.Tenure.PrimaryTenants)
            {
                var personModel = await _personApiGateway.GetPersonByIdAsync(responsibleTenant.Id, message.CorrelationId).ConfigureAwait(false);
                if (personModel is null)
                {
                    throw new EntityNotFoundException<Person>(responsibleTenant.Id);
                }

                var esTenure = _esEntityFactory.CreateQueryablePersonTenure(tenure);
                var esPerson = _esEntityFactory.CreatePerson(personModel);

                _ = await _esGateway.AddTenureToPersonAsync(esPerson, esTenure).ConfigureAwait(false);
            }
        }
    }
}
