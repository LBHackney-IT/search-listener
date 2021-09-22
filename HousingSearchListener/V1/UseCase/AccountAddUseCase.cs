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
    public class AccountAddUseCase : IMessageProcessing
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

            var account = await _accountApiGateway.GetAccountByIdAsync(message.EntityId).ConfigureAwait(false);
            if (account is null)
            {
                throw new EntityNotFoundException<Account>(message.EntityId);
            }

            var tenure = await _tenureApiGateway.GetTenureByIdAsync(account.TargetId).ConfigureAwait(false);
            if (tenure is null)
            {
                throw new EntityNotFoundException<TenureInformation>(account.TargetId);
            }
            tenure.TotalBalance = (double)account.AccountBalance;

            // Hanna Holasava
            // HouseHoldMembers can contains multiple responsible members
            var responsibleMembers = tenure.HouseholdMembers.Where(m => m.IsResponsible).ToList();
            if (responsibleMembers == null || responsibleMembers.Count == 0)
            {
                throw new MissedEntityDataException($"Tenure with id {message.EntityId} does not have any responsible household members.");
            }

            foreach (var responsibleMember in responsibleMembers)
            {
                if (!Guid.TryParse(responsibleMember.Id, out Guid personId))
                {
                    throw new FormatException(nameof(personId));
                }

                var personModel = await _personApiGateway.GetPersonByIdAsync(personId).ConfigureAwait(false);
                if (personModel is null)
                {
                    throw new EntityNotFoundException<Person>(personId);
                }

                var esTenure = _esEntityFactory.CreateTenure(tenure);
                var esPerson = _esEntityFactory.CreatePerson(personModel);

                _ = await _esGateway.AddTenureToPersonAsync(esPerson, esTenure).ConfigureAwait(false);
            }
        }
    }
}
