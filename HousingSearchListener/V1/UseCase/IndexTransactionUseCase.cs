﻿using Hackney.Core.Http;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Transactions;
using HousingSearchListener.V1.Factories.Interfaces;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class IndexTransactionUseCase : IIndexTransactionUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IFinancialTransactionApiGateway _financialTransactionApiGateway;
        private readonly ITransactionFactory _transactionFactory;

        public IndexTransactionUseCase(IEsGateway esGateway, IFinancialTransactionApiGateway financialTransactionApiGateway, ITransactionFactory transactionFactory)
        {
            _esGateway = esGateway;
            _financialTransactionApiGateway = financialTransactionApiGateway;
            _transactionFactory = transactionFactory;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Extract Transaction Target Id
            var transactionEventData = JsonSerializer.Deserialize<Transaction>(message?.EventData?.NewData?.ToString() ??
                                                                               string.Empty, JsonOptions.Create());

            // 2. Get Transaction from Financial Transaction API
            var transaction = await _financialTransactionApiGateway.GetTransactionByIdAsync(message.EntityId,
                    transactionEventData.TargetId, message.CorrelationId)
                .ConfigureAwait(false);

            if (transaction is null)
            {
                throw new EntityNotFoundException<Transaction>(message.EntityId);
            }

            // 3. Create Transaction Asset
            var esTransaction = _transactionFactory.CreateQueryableTransaction(transaction);
            await _esGateway.IndexTransaction(esTransaction).ConfigureAwait(false);
        }
    }
}
