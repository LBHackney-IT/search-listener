using AutoFixture;
using FluentAssertions;
using Hackney.Core.Http;
using Hackney.Shared.HousingSearch.Domain.Process;
using HousingSearchListener.V1.Gateway;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.Gateway
{
    [Collection("LogCall collection")]
    public class ProcessesApiGatewayTests
    {
        private readonly Mock<IApiGateway> _mockApiGateway;

        private static readonly Guid _id = Guid.NewGuid();
        private static readonly Guid _correlationId = Guid.NewGuid();

        private const string ApiName = "Processes";
        private const string ProcessesApiUrlKey = "ProcessesApiUrl";
        private const string ProcessesApiTokenKey = "ProcessesApiToken";

        private const string ProcessesApiRoute = "https://some-domain.com/api/";
        private const string ProcessesApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        public ProcessesApiGatewayTests()
        {
            _mockApiGateway = new Mock<IApiGateway>();

            _mockApiGateway.SetupGet(x => x.ApiName).Returns(ApiName);
            _mockApiGateway.SetupGet(x => x.ApiRoute).Returns(ProcessesApiRoute);
            _mockApiGateway.SetupGet(x => x.ApiToken).Returns(ProcessesApiToken);
        }

        private static string Route => $"{ProcessesApiRoute}/process/{_id}";

        [Fact]
        public void ConstructorTestInitialisesApiGateway()
        {
            new ProcessesApiGateway(_mockApiGateway.Object);
            _mockApiGateway.Verify(x => x.Initialise(ApiName, ProcessesApiUrlKey, ProcessesApiTokenKey, null),
                                   Times.Once);
        }

        [Fact]
        public void GetProcessByIdAsyncGetExceptionThrown()
        {
            var exMessage = "This is an exception";
            _mockApiGateway.Setup(x => x.GetByIdAsync<Process>(Route, _id, _correlationId))
                           .ThrowsAsync(new Exception(exMessage));

            var sut = new ProcessesApiGateway(_mockApiGateway.Object);
            Func<Task<Process>> func =
                async () => await sut.GetProcessByIdAsync(_id, _correlationId).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public async Task GetProcessByIdAsyncNotFoundReturnsNull()
        {
            var sut = new ProcessesApiGateway(_mockApiGateway.Object);
            var result = await sut.GetProcessByIdAsync(_id, _correlationId).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProcessByIdAsyncCallReturnsProcess()
        {
            var process = new Fixture().Create<Process>();

            _mockApiGateway.Setup(x => x.GetByIdAsync<Process>(Route, _id, _correlationId))
                           .ReturnsAsync(process);

            var sut = new ProcessesApiGateway(_mockApiGateway.Object);
            var result = await sut.GetProcessByIdAsync(_id, _correlationId).ConfigureAwait(false);

            result.Should().BeEquivalentTo(process);
        }
    }
}
