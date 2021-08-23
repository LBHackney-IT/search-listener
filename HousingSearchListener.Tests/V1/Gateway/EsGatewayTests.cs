﻿using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Gateway;
using Moq;
using Nest;
using System;
using System.Collections.Generic;
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

        private ESTenure CreateTenure()
        {
            return _fixture.Build<ESTenure>()
                           .With(x => x.StartDate, DateTime.UtcNow.AddMonths(-10).ToString())
                           .With(x => x.EndDate, (string)null)
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
            var tenure = CreateTenure();
            _mockEsClient.Setup(x => x.IndexAsync(It.IsAny<IndexRequest<ESTenure>>(), default(CancellationToken)))
                         .ReturnsAsync(indexResponse);
            var response = await _sut.IndexTenure(tenure).ConfigureAwait(false);

            response.Should().Be(indexResponse);
            _mockEsClient.Verify(x => x.IndexAsync(It.Is<IndexRequest<ESTenure>>(y => ValidateIndexRequest<ESTenure>(y, tenure)),
                                                   default(CancellationToken)), Times.Once);
        }

        [Fact]
        public async Task IndexTenureTestCallsEsClient()
        {
            var sut = new EsGateway(_testFixture.ElasticSearchClient);
            var tenure = CreateTenure();
            var response = await sut.IndexTenure(tenure).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Result.Should().Be(Result.Created);

            var result = await _testFixture.ElasticSearchClient
                                           .GetAsync<ESTenure>(tenure.Id, g => g.Index("tenures"))
                                           .ConfigureAwait(false);
            result.Source.Should().BeEquivalentTo(tenure);

            _cleanup.Add(async () => await _testFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("tenures", tenure.Id))
                                                                           .ConfigureAwait(false));
        }
    }
}
