using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Contract;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway.Interfaces;
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
    public class AddOrUpdateContractInAssetUseCaseTests
    {
        private readonly Mock<IAssetApiGateway> _mockAssetApi;
        private readonly Mock<IContractApiGateway> _mockContractApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly AddOrUpdateContractInAssetUseCase _sut;

        private readonly EntityEventSns _messageCreated;
        private readonly EntityEventSns _messageUpdated;
        private readonly EntityEventSns _messageAsset;
        private readonly Hackney.Shared.HousingSearch.Domain.Asset.Asset _Asset;
        private readonly Contract _Contract;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public AddOrUpdateContractInAssetUseCaseTests()
        {
            _fixture = new Fixture();

            _mockAssetApi = new Mock<IAssetApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _esEntityFactory = new ESEntityFactory();
            _sut = new AddOrUpdateContractInAssetUseCase(_mockEsGateway.Object,
                _mockContractApi.Object, _mockAssetApi.Object, _esEntityFactory);

            _messageAsset = CreateAssetMessage();
            _messageCreated = CreateContractCreatedEventMessage();
            _messageUpdated = CreateContractUpdatedEventMessage();
            _Asset = CreateAsset(_messageAsset.EntityId);
            _Contract = CreateContract(_messageCreated.EntityId);
        }

        private EntityEventSns CreateAssetMessage(string eventType = EventTypes.AssetCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private EntityEventSns CreateContractCreatedEventMessage(string eventType = EventTypes.ContractCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private EntityEventSns CreateContractUpdatedEventMessage(string eventType = EventTypes.ContractUpdatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private Hackney.Shared.HousingSearch.Domain.Asset.Asset CreateAsset(Guid entityId)
        {
            return _fixture.Build<Hackney.Shared.HousingSearch.Domain.Asset.Asset>()
                           .With(x => x.Id, entityId.ToString())
                           .Create();
        }

        private Contract CreateContract(Guid entityId)
        {
            return _fixture.Build<Contract>()
                           .With(x => x.Id, entityId.ToString())
                           .Create();
        }

        private bool VerifyContractIndexed(Contract esContract)
        {
            //esContract.Should().BeEquivalentTo(_esEntityFactory.CreateContract(_Contract));
            return true;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

    }
}
