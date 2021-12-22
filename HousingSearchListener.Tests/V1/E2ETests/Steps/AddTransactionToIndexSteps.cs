using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hackney.Shared.HousingSearch.Domain.Transactions;
using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;
using HousingSearchListener.V1.Domain.Transaction;
using HousingSearchListener.V1.Factories.QueryableFactories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Nest;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class AddTransactionToIndexSteps : BaseSteps
    {
        private readonly TransactionsFactory _transactionFactory = new TransactionsFactory();

        public AddTransactionToIndexSteps()
        {
            _eventType = EventTypes.TransactionCreatedEvent;
        }
        public async Task WhenTheFunctionIsTriggered(Guid tenureId, string eventType)
        {
            var eventMsg = CreateEvent(tenureId, eventType);
            await TriggerFunction(CreateMessage(eventMsg));
        }
        public void ThenATransactionNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<Transaction>));
            (_lastException as EntityNotFoundException<TransactionResponseObject>)?.Id.Should().Be(id);
        }

        public async Task ThenTheTransactionIndexIsUpdated(
            TransactionResponseObject transaction, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableTransaction>(transaction.Id, g => g.Index("transactions"))
                .ConfigureAwait(false);

            var transactionInIndex = result.Source;
            transactionInIndex.Should().BeEquivalentTo(_transactionFactory.CreateQueryableTransaction(transaction));
        }
    }
}
