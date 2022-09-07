using System;
using HousingSearchListener.Tests.V1.E2ETests.Fixtures;
using HousingSearchListener.Tests.V1.E2ETests.Steps;
using HousingSearchListener.V1.Boundary;
using TestStack.BDDfy;
using Xunit;

namespace HousingSearchListener.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "SQS Housing Search Listener",
        IWant = "a function to process the transaction created message",
        SoThat = "The transaction details are set in the transaction indexes")]
    [Collection("ElasticSearch collection")]
    public class AddTransactionToIndexTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly FinancialTransactionApiFixture _financialTransactionApiFixture;


        private readonly AddTransactionToIndexSteps _steps;

        public AddTransactionToIndexTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _financialTransactionApiFixture = new FinancialTransactionApiFixture();

            _steps = new AddTransactionToIndexSteps();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _financialTransactionApiFixture.Dispose();

                _disposed = true;
            }
        }
        [Theory]
        [InlineData(EventTypes.TransactionCreatedEvent)]
        public void TransactionNotFound(string eventType)
        {
            var id = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            this.Given(g => _financialTransactionApiFixture.GivenTheTransactionDoesNotExist(id, targetId))
                .When(w => _steps.WhenTheFunctionIsTriggered(id, eventType))
                .Then(t => _steps.ThenATransactionNotFoundExceptionIsThrown(id))
                .BDDfy();
        }
        [Theory]
        [InlineData(EventTypes.TransactionCreatedEvent)]
        public void TransactionFoundIndexSuccess(string eventType)
        {
            var id = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            this.Given(g => _financialTransactionApiFixture.GivenTheTransactionExists(id, targetId))
                .When(w => _steps.WhenTheFunctionIsTriggered(id, eventType))
                .Then(t => _steps.ThenTheTransactionIndexIsUpdated(_financialTransactionApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}
