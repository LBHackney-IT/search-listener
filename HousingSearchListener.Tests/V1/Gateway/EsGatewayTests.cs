using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Gateway;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using QueryableAsset = HousingSearchListener.V1.Domain.ElasticSearch.Asset.QueryableAsset;

namespace HousingSearchListener.Tests.V1.Gateway
{
    [Collection("ElasticSearch collection")]
    public class EsGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly Mock<IElasticClient> _mockEsClient;
        private readonly Mock<ILogger<EsGateway>> _mockLogger;
        private readonly EsGateway _sut;

        private readonly ElasticSearchFixture _testFixture;
        private readonly List<Action> _cleanup = new List<Action>();
        private const string DateFormat = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";

        public EsGatewayTests(ElasticSearchFixture testFixture)
        {
            _testFixture = testFixture;

            _mockEsClient = new Mock<IElasticClient>();
            _mockLogger = new Mock<ILogger<EsGateway>>();
            _sut = new EsGateway(_mockEsClient.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var action in _cleanup)
                    action();

                _disposed = true;
            }
        }

        private bool ValidateIndexRequest<T>(IndexRequest<T> actual, T obj) where T : class
        {
            actual.Document.Should().Be(obj);
            return true;
        }

        private QueryablePerson CreatePerson()
        {
            return _fixture.Build<QueryablePerson>()
                           .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString())
                           .Create();
        }

        private QueryableTenure CreateQueryableTenure()
        {
            return _fixture.Build<QueryableTenure>()
                           .With(x => x.Id, Guid.NewGuid().ToString())
                           .With(x => x.StartOfTenureDate, DateTime.UtcNow.AddMonths(-10).ToString())
                           .With(x => x.EndOfTenureDate, (string)null)
                           .With(x => x.HouseholdMembers, _fixture.Build<QueryableHouseholdMember>()
                                                                  .With(x => x.Id, Guid.NewGuid().ToString())
                                                                  .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-40).ToString(DateFormat))
                                                                  .With(x => x.PersonTenureType, "Tenant")
                                                                  .CreateMany(3).ToList())
                           .Create();
        }

        private QueryableAsset CreateQueryableAsset()
        {
            return _fixture.Build<QueryableAsset>()
                           .With(x => x.Id, Guid.NewGuid().ToString())
                           .Create();
        }

        [Fact]
        public void IndexPersonTestNullPersonThrows()
        {
            Func<Task<IndexResponse>> func = async () => await _sut.IndexPerson(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task IndexPersonTestCallsEsClientUsingMocks()
        {
            var indexResponse = _fixture.Create<IndexResponse>();
            var person = CreatePerson();
            _mockEsClient.Setup(x => x.IndexAsync(It.IsAny<IndexRequest<QueryablePerson>>(), default(CancellationToken)))
                         .ReturnsAsync(indexResponse);
            var response = await _sut.IndexPerson(person).ConfigureAwait(false);

            response.Should().Be(indexResponse);
            _mockEsClient.Verify(x => x.IndexAsync(It.Is<IndexRequest<QueryablePerson>>(y => ValidateIndexRequest<QueryablePerson>(y, person)),
                                                   default(CancellationToken)), Times.Once);
            _mockLogger.VerifyExact(LogLevel.Debug, $"Updating 'persons' index for person id {person.Id}", Times.Once());
        }

        [Fact]
        public async Task IndexPersonTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient, _mockLogger.Object);
            var person = CreatePerson();
            var response = await sut.IndexPerson(person).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Created);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<QueryablePerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);
            result.Source.Should().BeEquivalentTo(person);

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("persons", person.Id))
                                                                           .ConfigureAwait(false));
        }

        [Fact]
        public void IndexTenureTestNullTenureThrows()
        {
            Func<Task<IndexResponse>> func = async () => await _sut.IndexTenure(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task IndexTenureTestCallsEsClientUsingMocks()
        {
            var indexResponse = _fixture.Create<IndexResponse>();
            var tenure = CreateQueryableTenure();
            _mockEsClient.Setup(x => x.IndexAsync(It.IsAny<IndexRequest<QueryableTenure>>(), default(CancellationToken)))
                         .ReturnsAsync(indexResponse);
            var response = await _sut.IndexTenure(tenure).ConfigureAwait(false);

            response.Should().Be(indexResponse);
            _mockEsClient.Verify(x => x.IndexAsync(It.Is<IndexRequest<QueryableTenure>>(y => ValidateIndexRequest(y, tenure)),
                                                   default(CancellationToken)), Times.Once);
            _mockLogger.VerifyExact(LogLevel.Debug, $"Updating 'tenures' index for tenure id {tenure.Id}", Times.Once());
        }

        [Fact]
        public async Task IndexTenureTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient, _mockLogger.Object);
            var tenure = CreateQueryableTenure();
            var response = await sut.IndexTenure(tenure).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Created);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<QueryableTenure>(tenure.Id, g => g.Index("tenures"))
                                           .ConfigureAwait(false);
            result.Source.Should().BeEquivalentTo(tenure);

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("tenures", tenure.Id))
                                                                           .ConfigureAwait(false));
        }

        [Fact]
        public void IndexAssetTestNullTenureThrows()
        {
            Func<Task<IndexResponse>> func = async () => await _sut.IndexAsset(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task IndexAssetTestCallsEsClientUsingMocks()
        {
            var indexResponse = _fixture.Create<IndexResponse>();
            var asset = CreateQueryableAsset();
            _mockEsClient.Setup(x => x.IndexAsync(It.IsAny<IndexRequest<QueryableAsset>>(), default(CancellationToken)))
                         .ReturnsAsync(indexResponse);
            var response = await _sut.IndexAsset(asset).ConfigureAwait(false);

            response.Should().Be(indexResponse);
            _mockEsClient.Verify(x => x.IndexAsync(It.Is<IndexRequest<QueryableAsset>>(y => ValidateIndexRequest(y, asset)),
                                                   default(CancellationToken)), Times.Once);
            _mockLogger.VerifyExact(LogLevel.Debug, $"Updating 'assets' index for asset id {asset.Id}", Times.Once());
        }

        [Fact]
        public async Task IndexAssetTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient, _mockLogger.Object);
            var asset = CreateQueryableAsset();
            var response = await sut.IndexAsset(asset).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Created);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<QueryableAsset>(asset.Id, g => g.Index("assets"))
                                           .ConfigureAwait(false);
            result.Source.Should().BeEquivalentTo(asset);

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("assets", asset.Id))
                                                                           .ConfigureAwait(false));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetAssetByIdTestInvalidInputThrows(string id)
        {
            Func<Task<QueryableAsset>> func = async () => await _sut.GetAssetById(id).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        // The test below will not work because the ElasticSearchClient GetAsync call returns an un-mockable concrete class with readonly properties
        // so it cannot be set up correctly for testing purposes.
        //[Fact]
        //public async Task GetAssetByIdTestCallsEsClientUsingMocks()
        //{
        //    var asset = CreateQueryableAsset();
        //    var getResponse = _fixture.Build<GetResponse<QueryableAsset>>()
        //        .With(x => x.Id, asset.Id)
        //        .With(x => x.Source, asset)
        //        .With(x => x.Found, true)
        //        .Create();

        //    _mockEsClient.Setup(x => x.GetAsync<QueryableAsset>(It.IsAny<IGetRequest<QueryableAsset>>(),
        //                                        default(CancellationToken)))
        //                 .ReturnsAsync(getResponse);
        //    var response = await _sut.GetAssetById(asset.Id).ConfigureAwait(false);

        //    response.Should().Be(getResponse);
        //    _mockEsClient.Verify(x => x.GetAsync<QueryableAsset>(It.Is<IGetRequest<QueryableAsset>>(x => VerifyGetAssetRequest(x as GetRequest<QueryableAsset>, asset.Id)),
        //                                         default(CancellationToken)), 
        //                         Times.Once);
        //}
        //private bool VerifyGetAssetRequest(GetRequest<QueryableAsset> request, string id)
        //{
        //    request.Should().BeEquivalentTo(new GetRequest<QueryableAsset>("assets", id));
        //    return true;
        //}

        [Fact]
        public async Task GetAssetByIdTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient, _mockLogger.Object);
            var asset = CreateQueryableAsset();
            var request = new IndexRequest<QueryableAsset>(asset, "assets");
            await _testFixture.ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);

            var response = await sut.GetAssetById(asset.Id).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(asset);

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("assets", asset.Id))
                                                                           .ConfigureAwait(false));
        }
    }
}
