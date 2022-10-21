using AutoFixture;
using FluentAssertions;
using Hackney.Core.Http;
using Hackney.Shared.Asset.Domain;
using HousingSearchListener.V1.Gateway;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.Gateway
{
    [Collection("LogCall collection")]
    public class AssetApiGatewayTests
    {
        private readonly Mock<IApiGateway> _mockApiGateway;

        private static readonly Guid _id = Guid.NewGuid();
        private static readonly Guid _correlationId = Guid.NewGuid();

        private const string ApiName = "AssetInformationApi";
        private const string AssetApiUrlKey = "AssetApiUrl";
        private const string AssetApiTokenKey = "AssetApiToken";

        private const string AssetApiRoute = "https://some-domain.com/api/";
        private const string AssetApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        public AssetApiGatewayTests()
        {
            _mockApiGateway = new Mock<IApiGateway>();

            _mockApiGateway.SetupGet(x => x.ApiName).Returns(ApiName);
            _mockApiGateway.SetupGet(x => x.ApiRoute).Returns(AssetApiRoute);
            _mockApiGateway.SetupGet(x => x.ApiToken).Returns(AssetApiToken);
        }

        private static string Route => $"{AssetApiRoute}/assets/{_id}";

        [Fact]
        public void ConstructorTestInitialisesApiGateway()
        {
            new AssetApiGateway(_mockApiGateway.Object);
            _mockApiGateway.Verify(x => x.Initialise(ApiName, AssetApiUrlKey, AssetApiTokenKey, null, false),
                                   Times.Once);
        }

        [Fact]
        public void GetAssetByIdAsyncGetExceptionThrown()
        {
            var exMessage = "This is an exception";
            _mockApiGateway.Setup(x => x.GetByIdAsync<Asset>(Route, _id, _correlationId))
                           .ThrowsAsync(new Exception(exMessage));

            var sut = new AssetApiGateway(_mockApiGateway.Object);
            Func<Task<Hackney.Shared.HousingSearch.Domain.Asset.Asset>> func =
                async () => await sut.GetAssetByIdAsync(_id, _correlationId).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public async Task GetAssetByIdAsyncNotFoundReturnsNull()
        {
            var sut = new AssetApiGateway(_mockApiGateway.Object);
            var result = await sut.GetAssetByIdAsync(_id, _correlationId).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAssetByIdAsyncCallReturnsAsset()
        {
            var Asset = new Fixture().Create<Asset>();

            _mockApiGateway.Setup(x => x.GetByIdAsync<Asset>(Route, _id, _correlationId))
                           .ReturnsAsync(Asset);

            var sut = new AssetApiGateway(_mockApiGateway.Object);
            var result = await sut.GetAssetByIdAsync(_id, _correlationId).ConfigureAwait(false);

            result.Should().BeEquivalentTo(Asset);
        }
    }
}
