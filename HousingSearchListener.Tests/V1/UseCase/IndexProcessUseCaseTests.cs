using AutoFixture;
using FluentAssertions;
using Hackney.Core.Http;
using Hackney.Core.Sns;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Domain.Process;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase;
using Moq;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class IndexProcessUseCaseTests
    {
        private readonly Mock<IProcessesApiGateway> _mockProcessesApi;
        private readonly Mock<IPersonApiGateway> _mockPersonApi;
        private readonly Mock<ITenureApiGateway> _mockTenureApi;
        private readonly Mock<IAssetApiGateway> _mockAssetApi;

        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly IndexProcessUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly Process _process;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public IndexProcessUseCaseTests()
        {
            _fixture = new Fixture();

            _mockProcessesApi = new Mock<IProcessesApiGateway>();
            _mockPersonApi = new Mock<IPersonApiGateway>();
            _mockTenureApi = new Mock<ITenureApiGateway>();
            _mockAssetApi = new Mock<IAssetApiGateway>();

            _mockEsGateway = new Mock<IEsGateway>();
            _esEntityFactory = new ESEntityFactory();
            _sut = new IndexProcessUseCase(_mockEsGateway.Object,
                                           _mockProcessesApi.Object,
                                           _mockTenureApi.Object,
                                           _mockPersonApi.Object,
                                           _mockAssetApi.Object,
                                           _esEntityFactory);

            _message = CreateMessage();
            _process = CreateProcess(_message.EntityId);

        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.ProcessStartedEvent)
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

        [Fact]
        public void ThrowsErrorWithNullMessage()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsErrorfIfProcessesApiThrowsError()
        {
            var exMsg = "This is an error";
            _mockProcessesApi.Setup(x => x.GetProcessByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }


        [Fact]
        public void ThrowsErrorfIfProcessDoesNotExist()
        {
            _mockProcessesApi.Setup(x => x.GetProcessByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync((Process)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<Process>>();
        }

        private Action<Func<Task>> SetUpTargetEntityApiToReturnNull(TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.asset:
                    _mockAssetApi.Setup(x => x.GetAssetByIdAsync(_process.TargetId, _message.CorrelationId)).ReturnsAsync((Asset)null);
                    return (func) =>
                    {
                        func.Should().ThrowAsync<EntityNotFoundException<Asset>>();
                        _mockAssetApi.Verify(x => x.GetAssetByIdAsync(_process.TargetId, _message.CorrelationId), Times.Once());
                    };

                case TargetType.person:
                    _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_process.TargetId, _message.CorrelationId)).ReturnsAsync((Person)null);
                    return (func) =>
                    {
                        func.Should().ThrowAsync<EntityNotFoundException<Person>>();
                        _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_process.TargetId, _message.CorrelationId), Times.Once());
                    };

                case TargetType.tenure:
                    _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_process.TargetId, _message.CorrelationId)).ReturnsAsync((TenureInformation)null);
                    return (func) =>
                    {
                        func.Should().ThrowAsync<EntityNotFoundException<TenureInformation>>();
                        _mockTenureApi.Verify(x => x.GetTenureByIdAsync(_process.TargetId, _message.CorrelationId), Times.Once());
                    };

                default:
                    throw new Exception($"Unknown target type: {targetType}");
            }
        }

        [Theory]
        [InlineData(TargetType.asset)]
        [InlineData(TargetType.person)]
        [InlineData(TargetType.tenure)]
        public void ThrowsErrorfIfTargetEntityDoesNotExist(TargetType targetType)
        {
            _process.TargetType = targetType;
            var verifyTargetApiIsCalled = SetUpTargetEntityApiToReturnNull(targetType);

            _mockProcessesApi.Setup(x => x.GetProcessByIdAsync(_message.EntityId, _message.CorrelationId))
                             .ReturnsAsync(_process);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            verifyTargetApiIsCalled.Invoke(func);
        }

        [Fact]
        public void ThrowsErrorfIfEsGatewayThrowsError()
        {
            var exMsg = "This is the last error";
            _mockProcessesApi.Setup(x => x.GetProcessByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_process);
            _mockEsGateway.Setup(x => x.IndexProcess(It.IsAny<QueryableProcess>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        private Action SetUpTargetEntityApi(TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.asset:
                    var asset = _fixture.Build<Asset>().With(x => x.Id, _process.TargetId).Create();
                    _mockAssetApi.Setup(x => x.GetAssetByIdAsync(_process.TargetId, _message.CorrelationId)).ReturnsAsync(asset);
                    return () => _mockAssetApi.Verify(x => x.GetAssetByIdAsync(_process.TargetId, _message.CorrelationId), Times.Once());

                case TargetType.person:
                    var person = _fixture.Build<Person>().With(x => x.Id, _process.TargetId.ToString()).Create();
                    _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_process.TargetId, _message.CorrelationId)).ReturnsAsync(person);
                    return () => _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_process.TargetId, _message.CorrelationId), Times.Once());

                case TargetType.tenure:
                    var tenure = _fixture.Build<TenureInformation>().With(x => x.Id, _process.TargetId.ToString()).Create();
                    _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_process.TargetId, _message.CorrelationId)).ReturnsAsync(tenure);
                    return () => _mockTenureApi.Verify(x => x.GetTenureByIdAsync(_process.TargetId, _message.CorrelationId), Times.Once());

                default:
                    throw new Exception($"Unknown target type: {targetType}");
            }
        }

        private bool VerifyProcessIndexed(QueryableProcess esProcess)
        {
            var expectedProcess = _esEntityFactory.CreateProcess(_process);
            esProcess.Should().BeEquivalentTo(expectedProcess, c => c.Excluding(x => x.RelatedEntities));

            esProcess.RelatedEntities.Should().ContainSingle(x => x.Id == _process.TargetId.ToString());

            esProcess.RelatedEntities.RemoveAll(x => x.Id == _process.TargetId.ToString());
            foreach (var relatedEntity in esProcess.RelatedEntities)
            {
                var processRelatedEntity = _process.RelatedEntities.Find(x => x.Id.ToString() == relatedEntity.Id);
                relatedEntity.TargetType.Should().BeEquivalentTo(processRelatedEntity.TargetType);
                relatedEntity.SubType.Should().BeEquivalentTo(processRelatedEntity.SubType);
                relatedEntity.Description.Should().BeEquivalentTo(processRelatedEntity.Description);
            }
            return true;
        }

        [Theory]
        [InlineData(TargetType.asset)]
        [InlineData(TargetType.person)]
        [InlineData(TargetType.tenure)]
        public async Task InsertsIntoIndexCorrectly(TargetType targetType)
        {
            _process.TargetType = targetType;
            var verifyTargetApiIsCalled = SetUpTargetEntityApi(targetType);

            var clonedProcess = JsonConvert.DeserializeObject<Process>(JsonConvert.SerializeObject(_process));
            // need to clone process so that we can check that RelatedEntities is changed
            _mockProcessesApi.Setup(x => x.GetProcessByIdAsync(_message.EntityId, _message.CorrelationId))
                             .ReturnsAsync(clonedProcess);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexProcess(It.Is<QueryableProcess>(y => VerifyProcessIndexed(y))), Times.Once);
            verifyTargetApiIsCalled.Invoke();
        }
    }
}
