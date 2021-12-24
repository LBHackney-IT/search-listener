﻿using Hackney.Core.Logging;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Boundary.Response;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories.Interfaces;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class AccountUpdatedUseCase : IAccountUpdatedUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IAccountFactory _accountFactory;
        private readonly IAccountApiGateway _accountApiGateway;
        private readonly ITenureApiGateway _tenureApiGateway;

        public AccountUpdatedUseCase(IEsGateway esGateway, IAccountApiGateway accountApiGateway, ITenureApiGateway tenureApiGateway, IAccountFactory accountFactory)
        {
            _esGateway = esGateway;
            _accountApiGateway = accountApiGateway;
            _tenureApiGateway = tenureApiGateway;
            _accountFactory = accountFactory;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Account from Account API
            var account = await _accountApiGateway.GetAccountByIdAsync(message.EntityId, message.CorrelationId)
                                                  .ConfigureAwait(false);
            if (account is null)
            {
                throw new EntityNotFoundException<AccountResponse>(message.EntityId);
            }

            var esAccount = _accountFactory.ToQueryableAccount(account.ToDomain());
            await _esGateway.IndexAccount(esAccount);

            // #2 - Get the tenure
            var tenure = await _tenureApiGateway.GetTenureByIdAsync(account.TargetId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (tenure is null) throw new EntityNotFoundException<TenureInformation>(account.TargetId);

            var tasksList = new List<Task>();
            // Update the indexed asset
            tasksList.Add(UpdateAccountDetailsOnAsset(tenure.TenuredAsset.Id, account));
            // Update the indexed tenure
            tasksList.Add(UpdateAccountDetailsOnTenure(tenure.Id, account));
            // Update the indexed persons
            tasksList.Add(UpdateAccountDetailsOnPersons(tenure.HouseholdMembers.Select(x => x.Id), tenure.Id, account));

            // Do it all at once
            Task.WaitAll(tasksList.ToArray());
        }

        private async Task UpdateAccountDetailsOnAsset(string assetId, AccountResponse account)
        {
            var esAsset = await _esGateway.GetAssetById(assetId).ConfigureAwait(false);

            if (esAsset != null)
            {
                esAsset.Tenure.PaymentReference = account.PaymentReference;
                await _esGateway.IndexAsset(esAsset);
            }
        }

        private async Task UpdateAccountDetailsOnTenure(string tenureId, AccountResponse account)
        {
            var esTenure = await _esGateway.GetTenureById(tenureId).ConfigureAwait(false);
            if (esTenure != null)
            {
                esTenure.PaymentReference = account.PaymentReference;
                await _esGateway.IndexTenure(esTenure);
            }
        }

        private Task UpdateAccountDetailsOnPersons(IEnumerable<string> personIds, string tenureId, AccountResponse account)
        {
            var tasksList = personIds.Select(x => UpdateAccountDetailsOnPerson(x, tenureId, account));
            Task.WaitAll(tasksList.ToArray());
            return Task.CompletedTask;
        }

        private async Task UpdateAccountDetailsOnPerson(string personId, string tenureId, AccountResponse account)
        {
            var esPerson = await _esGateway.GetPersonById(personId).ConfigureAwait(false);
            if (esPerson != null)
            {
                var personTenure = esPerson.Tenures.First(x => x.Id == tenureId);

                personTenure.PaymentReference = account.PaymentReference;
                personTenure.TotalBalance = account.AccountBalance;

                await _esGateway.IndexPerson(esPerson);
            }
        }
    }
}