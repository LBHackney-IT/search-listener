using FluentAssertions;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.UseCase;
using HousingSearchListener.V1.UseCase.Interfaces;
using Moq;
using System;
using Xunit;

namespace HousingSearchListener.Tests.V1.Factories
{
    public class MessageHandlerFactoryTests
    {
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly Mock<IESEntityFactory> _mockEsEntityFactory;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly MessageHandlerFactory _sut;

        public MessageHandlerFactoryTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockEsGateway = new Mock<IEsGateway>();
            _mockEsEntityFactory = new Mock<IESEntityFactory>();
            _sut = new MessageHandlerFactory(_mockServiceProvider.Object);
        }

        [Theory]
        [InlineData(EventTypes.PersonCreatedEvent)]
        [InlineData(EventTypes.PersonUpdatedEvent)]
        public void ToMessageProcessorIndexPersonUseCase(EventTypes eventType)
        {
            Mock<IPersonApiGateway> mockPersonApiGateway = new Mock<IPersonApiGateway>();
            IndexPersonUseCase indexPersonUseCase = new IndexPersonUseCase(_mockEsGateway.Object, mockPersonApiGateway.Object, _mockEsEntityFactory.Object);

            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(indexPersonUseCase);

            var result = _sut.ToMessageProcessor(eventType);
            result.Should().NotBeNull();

            var isIMessageProcessing = result is IMessageProcessing;
            isIMessageProcessing.Should().BeTrue();

            var processor = Convert.ChangeType(result, typeof(IndexPersonUseCase));
            processor.Should().NotBeNull();
        }

        [Fact]
        public void ToMessageProcessorIndexTenureUseCase()
        {
            Mock<ITenureApiGateway> mockTenureApiGateway = new Mock<ITenureApiGateway>();
            IndexTenureUseCase indexTenureUseCase = new IndexTenureUseCase(_mockEsGateway.Object, mockTenureApiGateway.Object, _mockEsEntityFactory.Object);

            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(indexTenureUseCase);

            var result = _sut.ToMessageProcessor(EventTypes.TenureCreatedEvent);
            result.Should().NotBeNull();

            var isIMessageProcessing = result is IMessageProcessing;
            isIMessageProcessing.Should().BeTrue();

            var processor = Convert.ChangeType(result, typeof(IndexTenureUseCase));
            processor.Should().NotBeNull();
        }

        [Fact]
        public void ToMessageProcessorAccountAddUseCase()
        {
            Mock<IPersonApiGateway> mockPersonApiGateway = new Mock<IPersonApiGateway>();
            Mock<ITenureApiGateway> mockTenureApiGateway = new Mock<ITenureApiGateway>();
            AccountAddUseCase AccountAddUseCase = new AccountAddUseCase(_mockEsGateway.Object, mockTenureApiGateway.Object,
                mockPersonApiGateway.Object, _mockEsEntityFactory.Object);

            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(AccountAddUseCase);

            var result = _sut.ToMessageProcessor(EventTypes.AccountCreatedEvent);
            result.Should().NotBeNull();

            var isIMessageProcessing = result is IMessageProcessing;
            isIMessageProcessing.Should().BeTrue();

            var processor = Convert.ChangeType(result, typeof(AccountAddUseCase));
            processor.Should().NotBeNull();
        }

        [Fact]
        public void ToMessageProcessorAccountUpdateUseCase()
        {
            Mock<IPersonApiGateway> mockPersonApiGateway = new Mock<IPersonApiGateway>();
            Mock<ITenureApiGateway> mockTenureApiGateway = new Mock<ITenureApiGateway>();
            AccountUpdateUseCase accountUpdateUseCase = new AccountUpdateUseCase(_mockEsGateway.Object, mockTenureApiGateway.Object,
                mockPersonApiGateway.Object, _mockEsEntityFactory.Object);

            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(accountUpdateUseCase);

            var result = _sut.ToMessageProcessor(EventTypes.AccountUpdatedEvent);
            result.Should().NotBeNull();

            var isIMessageProcessing = result is IMessageProcessing;
            isIMessageProcessing.Should().BeTrue();

            var processor = Convert.ChangeType(result, typeof(AccountUpdateUseCase));
            processor.Should().NotBeNull();
        }

        [Fact]
        public void ToMessageProcessorPersonBalanceUpdatedUseCase()
        {
            Mock<IPersonApiGateway> mockPersonApiGateway = new Mock<IPersonApiGateway>();
            Mock<ITenureApiGateway> mockTenureApiGateway = new Mock<ITenureApiGateway>();
            Mock<IAccountApiGateway> mockAccountApiGateway = new Mock<IAccountApiGateway>();
            PersonBalanceUpdatedUseCase accountUpdateUseCase = new PersonBalanceUpdatedUseCase(_mockEsGateway.Object, mockTenureApiGateway.Object,
                mockPersonApiGateway.Object, mockAccountApiGateway.Object, _mockEsEntityFactory.Object);

            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(accountUpdateUseCase);

            var result = _sut.ToMessageProcessor(EventTypes.PersonBalanceUpdatedEvent);
            result.Should().NotBeNull();

            var isIMessageProcessing = result is IMessageProcessing;
            isIMessageProcessing.Should().BeTrue();

            var processor = Convert.ChangeType(result, typeof(PersonBalanceUpdatedUseCase));
            processor.Should().NotBeNull();
        }

        [Theory]
        [InlineData((EventTypes)42)]
        [InlineData((EventTypes)13)]
        public void ToMessageProcessorInvalidEventTypeThrowsArgumentException(EventTypes eventType)
        {
            Func<IMessageProcessing> func = () => _sut.ToMessageProcessor(eventType);
            func.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ToMessageProcessorGetServiceReturnsNullThrowsArgumentException()
        {
            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(null);

            Func<IMessageProcessing> func = () => _sut.ToMessageProcessor(EventTypes.AccountCreatedEvent);
            func.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ToMessageProcessorGetServiceReturnsInvalidTypeThrowsArgumentException()
        {
            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(new object { });

            Func<IMessageProcessing> func = () => _sut.ToMessageProcessor(EventTypes.AccountCreatedEvent);
            func.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ToMessageProcessorGetServiceThrowsException()
        {
            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
                .Throws(new Exception());

            Func<IMessageProcessing> func = () => _sut.ToMessageProcessor(EventTypes.AccountCreatedEvent);
            func.Should().Throw<Exception>();
        }
    }
}
