using System;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Transactions;
using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;
using HousingSearchListener.V1.Domain.Transaction;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Factories.Interfaces;
using HousingSearchListener.V1.Factories.QueryableFactories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase;
using Moq;
using Xunit;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class IndexTransactionUseCaseTests
    {
        private readonly Mock<IFinancialTransactionApiGateway> _mockTransactionApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly ITransactionFactory _transactionFactory;
        private readonly IndexTransactionUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly TransactionResponseObject _transaction;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();
        private static readonly Guid _targetId = Guid.NewGuid();

        public IndexTransactionUseCaseTests()
        {
            _fixture = new Fixture();

            _mockTransactionApi = new Mock<IFinancialTransactionApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _transactionFactory = new TransactionsFactory();
            _sut = new IndexTransactionUseCase(_mockEsGateway.Object,
                _mockTransactionApi.Object, _transactionFactory);

            _message = CreateMessage();
            _transaction = CreateTransaction(_message.EntityId);

        }
        private EntityEventSns CreateMessage(string eventType = EventTypes.TransactionCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                .With(x => x.EventType, eventType)
                .With(x => x.CorrelationId, _correlationId)
                .Create();
        }
        private TransactionResponseObject CreateTransaction(Guid entityId)
        {
            return _fixture.Build<TransactionResponseObject>()
                .With(x => x.Id, entityId)
                .Create();
        }

        private bool VerifyTransactionIndexed(QueryableTransaction esTransaction)
        {
            esTransaction.Should().BeEquivalentTo(esTransaction);
            return true;
        }
        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTransactionExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockTransactionApi.Setup(x => x.GetTransactionByIdAsync(_message.EntityId, _targetId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTransactionReturnsNullThrows()
        {
            _mockTransactionApi.Setup(x => x.GetTransactionByIdAsync(_message.EntityId, _targetId, _message.CorrelationId))
                                       .ReturnsAsync((TransactionResponseObject)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<TransactionResponseObject>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexTransactionExceptionThrows()
        {
            var exMsg = "This is the last error";
            _mockTransactionApi.Setup(x => x.GetTransactionByIdAsync(_message.EntityId, _targetId, _message.CorrelationId))
                                       .ReturnsAsync(_transaction);
            _mockEsGateway.Setup(x => x.IndexTransaction(It.IsAny<QueryableTransaction>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Theory]
        [InlineData(global::HousingSearchListener.V1.Boundary.EventTypes.TransactionCreatedEvent)]
        public async Task ProcessMessageAsyncTestIndexTransactionSuccess(string eventType)
        {
            _message.EventType = eventType;

            _mockTransactionApi.Setup(x => x.GetTransactionByIdAsync(_message.EntityId, _targetId, _message.CorrelationId))
                .ReturnsAsync(_transaction);
            var transactionData = _fixture.Build<Transaction>()
                .With(x => x.Id, _message.EntityId)
                .With(x => x.TargetId, _targetId)
                .Create();
            _message.EventData.NewData = JsonSerializer.Serialize(transactionData, JsonOptions.CreateJsonOptions());

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexTransaction(It.Is<QueryableTransaction>(y => VerifyTransactionIndexed(y))), Times.Once);
        }
    }
}
