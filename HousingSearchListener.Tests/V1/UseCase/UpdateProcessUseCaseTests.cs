using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Process;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;
using Hackney.Shared.Processes.Sns;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class UpdateProcessUseCaseTests
    {
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly UpdateProcessUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly QueryableProcess _process;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public UpdateProcessUseCaseTests()
        {
            _fixture = new Fixture();

            _mockEsGateway = new Mock<IEsGateway>();
            _esEntityFactory = new ESEntityFactory();
            _sut = new UpdateProcessUseCase(_mockEsGateway.Object, _esEntityFactory);

            _message = CreateMessage();
            _process = CreateProcess(_message.EntityId);
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.ProcessUpdatedEvent)
        {
            var newData = _fixture.Build<ProcessStateChangeData>()
                                  .With(x => x.State, "some state")
                                  .Create();

            var eventData = _fixture.Create<EventData>();
            eventData.NewData = newData;

            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .With(x => x.EventData, eventData)
                           .Create();
        }

        private QueryableProcess CreateProcess(Guid entityId)
        {
            return _fixture.Build<QueryableProcess>()
                           .With(x => x.Id, entityId.ToString())
                           .Create();
        }

        private bool VerifyProcessIndexed(QueryableProcess esProcess)
        {
            var newData = _message.EventData.NewData as ProcessStateChangeData;
            esProcess.State.Should().BeEquivalentTo(newData.State);
            esProcess.StateStartedAt.Should().BeEquivalentTo(newData.StateStartedAt.ToString());
            esProcess.Should().BeEquivalentTo(_process);
            return true;
        }

        [Fact]
        public void ProcessMessageAsyncTestThrowsErrorIfNullMessage()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }


        [Fact]
        public void ThrowsErrorfIfNewDataDoesNotContainStateChangeData()
        {
            _message.EventData = new EventData();
            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<ProcessStateChangeData>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestThrowsErrorOnIndexProcess()
        {
            var exMsg = "This is the last error";
            _mockEsGateway.Setup(x => x.IndexProcess(It.IsAny<QueryableProcess>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ThrowsErrorfIfProcessDoesNotExistInES()
        {
            _mockEsGateway.Setup(x => x.GetProcessById(It.IsAny<string>())).ReturnsAsync((QueryableProcess)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<QueryableProcess>>();
        }

        [Theory]
        [InlineData(EventTypes.ProcessUpdatedEvent)]
        public async Task ProcessMessageAsyncTestIndexProcessSuccess(string eventType)
        {
            _message.EventType = eventType;

            _mockEsGateway.Setup(x => x.GetProcessById(_message.EntityId.ToString())).ReturnsAsync(_process);
            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexProcess(It.Is<QueryableProcess>(y => VerifyProcessIndexed(y))), Times.Once);
        }
    }
}
