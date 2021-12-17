using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Transactions;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class IndexTransactionUseCase : IIndexTransactionUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IFinancialTransactionApiGateway _financialTransactionApiGateway;
        private readonly IESEntityFactory _esEntityFactory;

        public IndexTransactionUseCase(IEsGateway esGateway, IFinancialTransactionApiGateway financialTransactionApiGateway, IESEntityFactory esEntityFactory)
        {
            _esGateway = esGateway;
            _financialTransactionApiGateway = financialTransactionApiGateway;
            _esEntityFactory = esEntityFactory;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Extract Transaction Target Id
            //var transactionEventData = JsonSerializer.Deserialize<Transaction>(message?.EventData?.NewData?.ToString() ??
            //                                                                   string.Empty, JsonOptions.CreateJsonOptions());

            // 2. Get Transaction from Financial Transaction API
            var transaction = await _financialTransactionApiGateway.GetTransactionByIdAsync(message.EntityId,
                    Guid.Parse("b43a3500-876c-b3ab-1319-150810f027cd"), message.CorrelationId)
                .ConfigureAwait(false);

            if (transaction is null) throw new EntityNotFoundException<Transaction>(message.EntityId);

            // 3. Create Transaction Asset
            var esTransaction = _esEntityFactory.CreateQueryableTransaction(transaction);
            await _esGateway.IndexTransaction(esTransaction);
        }
    }
}
