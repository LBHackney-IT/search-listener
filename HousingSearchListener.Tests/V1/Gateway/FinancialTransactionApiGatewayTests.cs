using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Domain.Transaction;
using HousingSearchListener.V1.Gateway;
using Moq;
using Xunit;

namespace HousingSearchListener.Tests.V1.Gateway
{
    [Collection("LogCall collection")]
    public class FinancialTransactionApiGatewayTests
    {
        private readonly Mock<INewtonsoftApiGateway> _mockApiGateway;

        private static readonly Guid _id = Guid.NewGuid();
        private static readonly Guid _targetId = Guid.NewGuid();
        private static readonly Guid _correlationId = Guid.NewGuid();
        private const string FinancialTransactionApiRoute = "https://some-domain.com/api/v1";
        private const string FinancialTransactionApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        private const string ApiName = "FinancialTransaction";
        private const string FinancialTransactionUrlKey = "FinancialTransactionApiUrl";
        private const string FinancialTransactionTokenKey = "FinancialTransactionApiToken";

        public FinancialTransactionApiGatewayTests()
        {
            _mockApiGateway = new Mock<INewtonsoftApiGateway>();
            _mockApiGateway.SetupGet(x => x.ApiName).Returns(ApiName);
            _mockApiGateway.SetupGet(x => x.ApiRoute).Returns(FinancialTransactionApiRoute);
            _mockApiGateway.SetupGet(x => x.ApiToken).Returns(FinancialTransactionApiToken);
        }
        private static string Route => $"{FinancialTransactionApiRoute}/transactions/{_id}?targetId={_targetId}";

        [Fact]
        public void ConstructorTestInitialisesApiGateway()
        {
            new FinancialTransactionApiGateway(_mockApiGateway.Object);
            _mockApiGateway.Verify(x => x.Initialise(ApiName, FinancialTransactionUrlKey, FinancialTransactionTokenKey, null),
                Times.Once);
        }

        [Fact]
        public void GetTransactionByIdAsyncExceptionThrown()
        {
            var exMessage = "This is an exception";
            _mockApiGateway.Setup(x => x.GetByIdAsync<TransactionResponseObject>(Route, _id, _correlationId))
                .ThrowsAsync(new Exception(exMessage));

            var sut = new FinancialTransactionApiGateway(_mockApiGateway.Object);
            Func<Task<TransactionResponseObject>> func =
                async () => await sut.GetTransactionByIdAsync(_id, _targetId, _correlationId).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public async Task GetTransactionByIdAsyncNotFoundReturnsNull()
        {
            var sut = new FinancialTransactionApiGateway(_mockApiGateway.Object);
            var result = await sut.GetTransactionByIdAsync(_id, _targetId, _correlationId).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTransactionByIdAsyncCallReturnsTransaction()
        {
            var transaction = new Fixture().Create<TransactionResponseObject>();

            _mockApiGateway.Setup(x => x.GetByIdAsync<TransactionResponseObject>(Route, _id, _correlationId))
                .ReturnsAsync(transaction);

            var sut = new FinancialTransactionApiGateway(_mockApiGateway.Object);
            var result = await sut.GetTransactionByIdAsync(_id, _targetId, _correlationId).ConfigureAwait(false);

            result.Should().BeEquivalentTo(transaction);
        }
    }
}
