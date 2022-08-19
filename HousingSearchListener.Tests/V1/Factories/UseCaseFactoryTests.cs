using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.UseCase.Interfaces;
using Moq;
using System;
using Xunit;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.Factories
{
    public class UseCaseFactoryTests
    {
        private readonly Fixture _fixture = new Fixture();
        private EntityEventSns _event;
        private readonly Mock<IServiceProvider> _mockServiceProvider;

        public UseCaseFactoryTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();

            _event = ConstructEvent(EventTypes.PersonCreatedEvent);
        }

        private EntityEventSns ConstructEvent(string eventType)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .Create();
        }

        [Fact]
        public void CreateUseCaseForMessageTestNullEventThrows()
        {
            Action act = () => UseCaseFactory.CreateUseCaseForMessage(null, _mockServiceProvider.Object);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateUseCaseForMessageTestNullServiceProviderThrows()
        {
            Action act = () => UseCaseFactory.CreateUseCaseForMessage(_event, null);
            act.Should().Throw<ArgumentNullException>();
        }

        private void TestMessageProcessingCreation<T>(EntityEventSns eventObj) where T : class, IMessageProcessing
        {
            var mockProcessor = new Mock<T>();
            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(mockProcessor.Object);

            var result = UseCaseFactory.CreateUseCaseForMessage(eventObj, _mockServiceProvider.Object);
            result.Should().NotBeNull();
            _mockServiceProvider.Verify(x => x.GetService(typeof(T)), Times.Once);
        }

        [Fact]
        public void CreateUseCaseForMessageTestUnknownEventThrows()
        {
            _event = ConstructEvent("UnknownEvent");

            Action act = () => UseCaseFactory.CreateUseCaseForMessage(_event, _mockServiceProvider.Object);
            act.Should().Throw<ArgumentException>().WithMessage($"Unknown event type: {_event.EventType}");
            _mockServiceProvider.Verify(x => x.GetService(It.IsAny<Type>()), Times.Never);
        }

        [Fact]
        public void CreateUseCaseForMessageTestPersonCreatedEvent()
        {
            _event = ConstructEvent(EventTypes.PersonCreatedEvent);
            TestMessageProcessingCreation<IIndexCreatePersonUseCase>(_event);
        }

        [Fact]
        public void CreateUseCaseForMessageTestPersonUpdatedEvent()
        {
            _event = ConstructEvent(EventTypes.PersonUpdatedEvent);
            TestMessageProcessingCreation<IIndexUpdatePersonUseCase>(_event);
        }

        [Theory]
        [InlineData(EventTypes.TenureCreatedEvent)]
        [InlineData(EventTypes.TenureUpdatedEvent)]
        public void CreateUseCaseForMessageTestTenureEvents(string eventType)
        {
            _event = ConstructEvent(eventType);
            TestMessageProcessingCreation<IIndexTenureUseCase>(_event);
        }

        [Fact]
        public void CreateUseCaseForMessageTestAddPersonToTenureEvent()
        {
            _event = ConstructEvent(EventTypes.PersonAddedToTenureEvent);
            TestMessageProcessingCreation<IAddPersonToTenureUseCase>(_event);
        }

        [Fact]
        public void CreateUseCaseForMessageTestPersonRemovedFromTenureEvent()
        {
            _event = ConstructEvent(EventTypes.PersonRemovedFromTenureEvent);
            TestMessageProcessingCreation<IRemovePersonFromTenureUseCase>(_event);
        }

        [Fact]
        public void CreateUseCaseForMessageTestAccountCreatedEvent()
        {
            _event = ConstructEvent(EventTypes.AccountCreatedEvent);
            TestMessageProcessingCreation<IUpdateAccountDetailsUseCase>(_event);
        }
        [Fact]
        public void CreateUseCaseForMessageTestTransactionCreatedEvent()
        {
            _event = ConstructEvent(EventTypes.TransactionCreatedEvent);
            TestMessageProcessingCreation<IIndexTransactionUseCase>(_event);
        }

        [Fact]
        public void CreateUseCaseForMessageTestAssetCreatedEvent()
        {
            _event = ConstructEvent(EventTypes.AssetCreatedEvent);
            TestMessageProcessingCreation<IIndexCreateAssetUseCase>(_event);
        }

        [Fact]
        public void CreateUseCaseForMessageTestAssetUpdatedEvent()
        {
            _event = ConstructEvent(EventTypes.AssetUpdatedEvent);
            TestMessageProcessingCreation<IUpdateAssetUseCase>(_event);
        }
    }
}
