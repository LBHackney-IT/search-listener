using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Process;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class UpdateProcessUseCaseTests
    {
        private readonly Mock<IProcessesApiGateway> _mockProcessesApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly UpdateProcessUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly Process _Process;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public UpdateProcessUseCaseTests()
        {
            _fixture = new Fixture();

            _mockProcessesApi = new Mock<IProcessesApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _esEntityFactory = new ESEntityFactory();
            _sut = new UpdateProcessUseCase(_mockEsGateway.Object,
                _mockProcessesApi.Object, _esEntityFactory);

            _message = CreateMessage();
            _Process = CreateProcess(_message.EntityId);
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.ProcessUpdatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private Process CreateProcess(Guid entityId)
        {
            return _fixture.Build<Process>()
                           .With(x => x.Id, entityId)
                           .Create();
        }

        private bool VerifyProcessIndexed(QueryableProcess esProcess)
        {
            esProcess.Should().BeEquivalentTo(_esEntityFactory.CreateProcess(_Process));
            return true;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetProcessExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockProcessesApi.Setup(x => x.GetProcessByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetAssetReturnsNullThrows()
        {
            _mockProcessesApi.Setup(x => x.GetProcessByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync((Process)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<Process>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexProcessExceptionThrows()
        {
            var exMsg = "This is the last error";
            _mockProcessesApi.Setup(x => x.GetProcessByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_Process);
            _mockEsGateway.Setup(x => x.IndexProcess(It.IsAny<QueryableProcess>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Theory]
        [InlineData(EventTypes.ProcessUpdatedEvent)]
        public async Task ProcessMessageAsyncTestIndexProcessSuccess(string eventType)
        {
            _message.EventType = eventType;

            _mockProcessesApi.Setup(x => x.GetProcessByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_Process);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexProcess(It.Is<QueryableProcess>(y => VerifyProcessIndexed(y))), Times.Once);
        }
    }
}
