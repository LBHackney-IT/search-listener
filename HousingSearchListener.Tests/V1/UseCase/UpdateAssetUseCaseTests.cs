using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
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
    public class UpdateAssetUseCaseTests
    {
        private readonly Mock<IAssetApiGateway> _mockAssetApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly IndexCreateAssetUseCase _create;
        private readonly UpdateAssetUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly QueryableAsset _asset;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public UpdateAssetUseCaseTests()
        {
            _fixture = new Fixture();

            _mockAssetApi = new Mock<IAssetApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _esEntityFactory = new ESEntityFactory();
            _create = new IndexCreateAssetUseCase(_mockEsGateway.Object,
                _mockAssetApi.Object, _esEntityFactory);

            _sut = new UpdateAssetUseCase(_mockEsGateway.Object,
                _mockAssetApi.Object, _esEntityFactory);

            _message = CreateMessage();
            _asset = CreateAsset(_message.EntityId);
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.AssetUpdatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private QueryableAsset CreateAsset(Guid entityId)
        {
            return _fixture.Build<QueryableAsset>()
                           .With(x => x.Id, entityId.ToString())
                           .Create();
        }

        private bool VerifyAssetIndexed(QueryableAsset esAsset)
        {
            esAsset.Should().BeEquivalentTo(_esEntityFactory.CreateAsset(_asset));
            return true;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetAssetExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockAssetApi.Setup(x => x.GetAssetByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetAssetReturnsNullThrows()
        {
            _mockAssetApi.Setup(x => x.GetAssetByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync((QueryableAsset)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<QueryableAsset>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexAssetExceptionThrows()
        {
            var exMsg = "This is the last error";
            _mockAssetApi.Setup(x => x.GetAssetByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_asset);
            _mockEsGateway.Setup(x => x.IndexAsset(It.IsAny<QueryableAsset>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestIndexSuccess()
        {
            var message = CreateMessage(EventTypes.AssetCreatedEvent);
            var asset = CreateAsset(message.EntityId);
            _mockAssetApi.Setup(x => x.GetAssetByIdAsync(message.EntityId, message.CorrelationId))
                .ReturnsAsync(asset);

            await _create.ProcessMessageAsync(message).ConfigureAwait(false);

            message.EventType = EventTypes.AssetUpdatedEvent;

            await _sut.ProcessMessageAsync(message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.GetAssetById(asset.Id), Times.Once());

            _mockEsGateway.Verify(x => x.IndexAsset(It.Is<QueryableAsset>(y => VerifyAssetIndexed(y))), Times.Once);

        }
    }
}
