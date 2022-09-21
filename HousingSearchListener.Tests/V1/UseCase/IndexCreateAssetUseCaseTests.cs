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
    public class IndexCreateAssetUseCaseTests
    {
        private readonly Mock<IAssetApiGateway> _mockAssetApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly IndexCreateAssetUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly Asset _Asset;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public IndexCreateAssetUseCaseTests()
        {
            _fixture = new Fixture();

            _mockAssetApi = new Mock<IAssetApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _esEntityFactory = new ESEntityFactory();
            _sut = new IndexCreateAssetUseCase(_mockEsGateway.Object,
                _mockAssetApi.Object, _esEntityFactory);

            _message = CreateMessage();
            _Asset = CreateAsset(_message.EntityId);
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.AssetCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private Asset CreateAsset(Guid entityId)
        {
            return _fixture.Build<Asset>()
                           .With(x => x.Id, entityId)
                           .Create();
        }

        private bool VerifyAssetIndexed(QueryableAsset esAsset)
        {
            esAsset.Should().BeEquivalentTo(_esEntityFactory.CreateAsset(_Asset));
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
                                       .ReturnsAsync((Asset)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<Asset>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexAssetExceptionThrows()
        {
            var exMsg = "This is the last error";
            _mockAssetApi.Setup(x => x.GetAssetByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_Asset);
            _mockEsGateway.Setup(x => x.IndexAsset(It.IsAny<QueryableAsset>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Theory]
        [InlineData(EventTypes.AssetCreatedEvent)]
        public async Task ProcessMessageAsyncTestIndexAssetSuccess(string eventType)
        {
            _message.EventType = eventType;

            _mockAssetApi.Setup(x => x.GetAssetByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_Asset);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexAsset(It.Is<QueryableAsset>(y => VerifyAssetIndexed(y))), Times.Once);
        }
    }
}
