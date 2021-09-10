using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Domain.Account;
using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Gateway;
using Moq;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.Gateway
{
    [Collection("ElasticSearch collection")]
    public class EsGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly Mock<IElasticClient> _mockEsClient;
        private readonly EsGateway _sut;

        private readonly ElasticSearchFixture _testFixture;
        private readonly List<Action> _cleanup = new List<Action>();

        public EsGatewayTests(ElasticSearchFixture testFixture)
        {
            _testFixture = testFixture;

            _mockEsClient = new Mock<IElasticClient>();
            _sut = new EsGateway(_mockEsClient.Object);
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

        private ESPerson CreatePerson()
        {
            return _fixture.Build<ESPerson>()
                           .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString())
                           .Create();
        }

        private QueryableTenure CreateQueryableTenure()
        {
            return _fixture.Build<QueryableTenure>()
                           .With(x => x.StartOfTenureDate, DateTime.UtcNow.AddMonths(-10).ToString())
                           .With(x => x.EndOfTenureDate, (string)null)
                           .Create();
        }

        private Account CreateAccount()
        {
            return _fixture.Build<Account>()
                           .With(x => x.AccountBalance, 42M)
                           .Create();
        }

        private ESPersonTenure CreateEsPersonTenure()
        {
            return _fixture.Build<ESPersonTenure>().Create();
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
            _mockEsClient.Setup(x => x.IndexAsync(It.IsAny<IndexRequest<ESPerson>>(), default(CancellationToken)))
                         .ReturnsAsync(indexResponse);
            var response = await _sut.IndexPerson(person).ConfigureAwait(false);

            response.Should().Be(indexResponse);
            _mockEsClient.Verify(x => x.IndexAsync(It.Is<IndexRequest<ESPerson>>(y => ValidateIndexRequest<ESPerson>(y, person)),
                                                   default(CancellationToken)), Times.Once);
        }

        [Fact]
        public async Task IndexPersonTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var person = CreatePerson();
            var response = await sut.IndexPerson(person).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Created);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
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
        }

        [Fact]
        public async Task IndexTenureTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
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
        public async Task UpdatePersonBalanceTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var person = CreatePerson();
            var account = CreateAccount();

            var createdResponse = await sut.IndexPerson(person).ConfigureAwait(false);
            createdResponse.Should().NotBeNull();
            createdResponse.Result.Should().Be(Result.Created);

            var createdResult = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            createdResult.Source.Should().BeEquivalentTo(person);

            var response = await sut.UpdatePersonBalanceAsync(person, account).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Updated);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            result.Source.Should().BeEquivalentTo(person, options => options.Excluding(_ => _.TotalBalance));

            result.Source.TotalBalance.Should().Be(account.AccountBalance);

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("persons", person.Id))
                                                                           .ConfigureAwait(false));
        }

        [Fact]
        public async Task UpdatePersonBalanceNonExistPersonResultCreated()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var person = CreatePerson();
            var account = CreateAccount();

            var response = await sut.UpdatePersonBalanceAsync(person, account).ConfigureAwait(false);

            response.Result.Should().NotBeNull();
            response.Result.Should().Be(Result.Created);
        }

        [Fact]
        public async Task UpdatePersonBalanceInvalidGuidResultCreated()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var person = CreatePerson();
            var account = CreateAccount();

            var createdResponse = await sut.IndexPerson(person).ConfigureAwait(false);
            createdResponse.Should().NotBeNull();
            createdResponse.Result.Should().Be(Result.Created);

            var createdResult = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            createdResult.Source.Should().BeEquivalentTo(person);

            person.Id = Guid.NewGuid().ToString();
            var response = await sut.UpdatePersonBalanceAsync(person, account).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Created);

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("persons", person.Id))
                                                                           .ConfigureAwait(false));
        }

        [Fact]
        public async Task AddTenureToPersonIndexTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var person = CreatePerson();
            var tenure = CreateEsPersonTenure();

            var createdResponse = await sut.IndexPerson(person).ConfigureAwait(false);
            createdResponse.Should().NotBeNull();
            createdResponse.Result.Should().Be(Result.Created);

            var createdResult = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            createdResult.Source.Should().BeEquivalentTo(person);

            var response = await sut.AddTenureToPersonIndexAsync(person, tenure).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Updated);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            var personResult = result.Source;

            personResult.Should().BeEquivalentTo(person, options => options.Excluding(_ => _.Tenures));

            personResult.Tenures.Should().NotBeEmpty();
            personResult.Tenures.FindLast(_ => _.Id.Equals(tenure.Id)).Should().BeEquivalentTo(tenure);

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("persons", person.Id))
                                                                           .ConfigureAwait(false));
        }

        [Fact]
        public async Task AddTenureToPersonIndexIdenticalTenureThrowsArgumentException()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var person = CreatePerson();
            var tenure = CreateEsPersonTenure();

            var createdResponse = await sut.IndexPerson(person).ConfigureAwait(false);
            createdResponse.Should().NotBeNull();
            createdResponse.Result.Should().Be(Result.Created);

            var createdResult = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            createdResult.Source.Should().BeEquivalentTo(person);

            var response = await sut.AddTenureToPersonIndexAsync(person, tenure).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Updated);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            var personResult = result.Source;

            personResult.Should().BeEquivalentTo(person, options => options.Excluding(_ => _.Tenures));

            personResult.Tenures.Should().NotBeEmpty();
            personResult.Tenures.FindLast(_ => _.Id.Equals(tenure.Id)).Should().BeEquivalentTo(tenure);

            Func<Task<UpdateResponse<Person>>> act = async () => await sut.AddTenureToPersonIndexAsync(person, tenure).ConfigureAwait(false);

            act.Should().Throw<ArgumentException>();

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("persons", person.Id))
                                                                           .ConfigureAwait(false));
        }

        [Fact]
        public async Task AddTenureToPersonIndexNonExistPersonResultCreated()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var person = CreatePerson();
            var tenure = CreateEsPersonTenure();

            var response = await sut.AddTenureToPersonIndexAsync(person, tenure).ConfigureAwait(false);

            response.Result.Should().NotBeNull();
            response.Result.Should().Be(Result.Created);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            var personResult = result.Source;
            personResult.Tenures.Should().BeEquivalentTo(person.Tenures);
        }

        [Fact]
        public async Task UpdatePersonAccountTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var person = CreatePerson();
            var tenure = CreateEsPersonTenure();
            tenure.Id = person.Tenures.FirstOrDefault().Id;

            var createdResponse = await sut.IndexPerson(person).ConfigureAwait(false);
            createdResponse.Should().NotBeNull();
            createdResponse.Result.Should().Be(Result.Created);

            var createdResult = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            createdResult.Source.Should().BeEquivalentTo(person);

            var response = await sut.UpdatePersonAccountAsync(person, tenure).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Updated);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            var personResult = result.Source;
            personResult.Should().BeEquivalentTo(person, options => options.Excluding(_ => _.Tenures));

            var updatedTenure = personResult.Tenures.Where(x => x.Id.Equals(tenure.Id)).FirstOrDefault();
            updatedTenure.Should().NotBeNull();
            updatedTenure.TotalBalance.Should().Be(tenure.TotalBalance);

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("persons", person.Id))
                                                                           .ConfigureAwait(false));
        }

        [Fact]
        public void UpdatePersonAccountNonExistTenureThrowsArgumentException()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var person = CreatePerson();
            var tenure = CreateEsPersonTenure();

            Func<Task<UpdateResponse<Person>>> act = async () => await sut.UpdatePersonAccountAsync(person, tenure).ConfigureAwait(false);

            act.Should().NotBeNull();
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task UpdatePersonAccountNonExistPersonResultCreated()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var person = CreatePerson();
            var tenure = person.Tenures.FirstOrDefault();

            var response = await sut.UpdatePersonAccountAsync(person, tenure).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Created);
        }
    }
}
