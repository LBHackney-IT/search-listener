using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class IndexTenureUseCaseTests
    {
        private readonly Mock<ITenureApiGateway> _mockTenureApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly IndexTenureUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly TenureInformation _tenure;
        private readonly QueryableAsset _asset;
        private readonly List<QueryablePerson> _persons;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();
        private const string DateFormat = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";

        public IndexTenureUseCaseTests()
        {
            _fixture = new Fixture();

            _mockTenureApi = new Mock<ITenureApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _esEntityFactory = new ESEntityFactory();
            _sut = new IndexTenureUseCase(_mockEsGateway.Object,
                _mockTenureApi.Object, _esEntityFactory);

            _message = CreateMessage();
            _tenure = CreateTenure(_message.EntityId);
            _asset = CreateAsset(_tenure.TenuredAsset.Id);
            _persons = CreatePersons(_tenure);
        }

        private List<QueryablePerson> CreatePersons(TenureInformation tenure)
        {
            var persons = new List<QueryablePerson>();
            foreach (var hm in tenure.HouseholdMembers)
                persons.Add(_fixture.Build<QueryablePerson>()
                                    .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString(DateFormat))
                                    .With(x => x.Id, hm.Id)
                                    .With(x => x.Tenures,
                                          _fixture.Build<QueryablePersonTenure>()
                                                  .With(y => y.AssetFullAddress, tenure.TenuredAsset.FullAddress)
                                                  .With(y => y.EndDate, (string)null)
                                                  .With(y => y.Id, tenure.Id)
                                                  .With(y => y.PaymentReference, (string)null)
                                                  .With(y => y.StartDate, (string)null)
                                                  .With(y => y.Type, tenure.TenureType.Description)
                                                  .CreateMany(1).ToList())
                                    .Create());
            return persons;
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.TenureCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private TenureInformation CreateTenure(Guid entityId)
        {
            return _fixture.Build<TenureInformation>()
                           .With(x => x.Id, entityId.ToString())
                           .With(x => x.StartOfTenureDate, DateTime.UtcNow.AddMonths(-6).ToString(DateFormat))
                           .With(x => x.EndOfTenureDate, DateTime.UtcNow.AddYears(6).ToString(DateFormat))
                           .With(x => x.TenuredAsset, _fixture.Build<TenuredAsset>()
                                                              .With(x => x.Id, Guid.NewGuid().ToString())
                                                              .Create())
                           .Create();
        }

        private QueryableAsset CreateAsset(string id)
        {
            return _fixture.Build<QueryableAsset>()
                           .With(x => x.Id, id)
                           .Create();
        }

        private bool VerifyTenureIndexed(QueryableTenure esTenure)
        {
            esTenure.Should().BeEquivalentTo(_esEntityFactory.CreateQueryableTenure(_tenure));
            return true;
        }

        private bool VerifyAssetIndexed(QueryableAsset asset)
        {
            asset.Should().BeEquivalentTo(_asset, c => c.Excluding(x => x.Tenure));
            asset.Tenure.EndOfTenureDate.Should().Be(_tenure.EndOfTenureDate);
            asset.Tenure.Id.Should().Be(_tenure.Id);
            asset.Tenure.PaymentReference.Should().Be(_tenure.PaymentReference);
            asset.Tenure.StartOfTenureDate.Should().Be(_tenure.StartOfTenureDate);
            asset.Tenure.Type.Should().Be(_tenure.TenureType.Description);
            return true;
        }

        private bool VerifyPersonIndexed(QueryablePerson person)
        {
            person.Should().BeEquivalentTo(_persons.First(x => x.Id == person.Id), c => c.Excluding(x => x.Tenures));
            var pt = person.Tenures.First();
            pt.AssetFullAddress.Should().Be(_tenure.TenuredAsset.FullAddress);
            pt.EndDate.Should().Be(_tenure.EndOfTenureDate);
            pt.Id.Should().Be(_tenure.Id);
            pt.PaymentReference.Should().Be(_tenure.PaymentReference);
            pt.StartDate.Should().Be(_tenure.StartOfTenureDate);
            pt.Type.Should().Be(_tenure.TenureType.Description);
            return true;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTenureExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTenureReturnsNullThrows()
        {
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync((TenureInformation)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<TenureInformation>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexTenureExceptionThrows()
        {
            var exMsg = "This is the last error";
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);
            _mockEsGateway.Setup(x => x.IndexTenure(It.IsAny<QueryableTenure>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Theory]
        [InlineData(EventTypes.TenureCreatedEvent)]
        [InlineData(EventTypes.TenureUpdatedEvent)]
        public void ProcessMessageAsyncTestIndexTenureNoAssetThrows(string eventType)
        {
            _message.EventType = eventType;

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<AssetNotIndexedException>();

            _mockEsGateway.Verify(x => x.IndexTenure(It.Is<QueryableTenure>(y => VerifyTenureIndexed(y))), Times.Never);
            _mockEsGateway.Verify(x => x.IndexAsset(It.IsAny<QueryableAsset>()), Times.Never);
        }

        [Theory]
        [InlineData(EventTypes.TenureCreatedEvent)]
        [InlineData(EventTypes.TenureUpdatedEvent)]
        public void ProcessMessageAsyncTestGetPersonReturnsNullThrows(string eventType)
        {
            _message.EventType = eventType;

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync((TenureInformation)null);

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);
            _mockEsGateway.Setup(x => x.GetAssetById(_tenure.TenuredAsset.Id)).ReturnsAsync(_asset);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<QueryablePerson>>();
        }

        [Theory]
        [InlineData(EventTypes.TenureCreatedEvent)]
        [InlineData(EventTypes.TenureUpdatedEvent)]
        public async Task ProcessMessageAsyncTestIndexTenureSuccess(string eventType)
        {
            _message.EventType = eventType;

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);
            _mockEsGateway.Setup(x => x.GetAssetById(_tenure.TenuredAsset.Id)).ReturnsAsync(_asset);
            foreach (var p in _persons)
                _mockEsGateway.Setup(x => x.GetPersonById(p.Id)).ReturnsAsync(_persons.First(z => z.Id == p.Id));

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexTenure(It.Is<QueryableTenure>(y => VerifyTenureIndexed(y))), Times.Once);
            _mockEsGateway.Verify(x => x.IndexAsset(It.Is<QueryableAsset>(y => VerifyAssetIndexed(y))), Times.Once);
            foreach (var p in _persons)
                _mockEsGateway.Verify(x => x.IndexPerson(It.Is<QueryablePerson>(y => VerifyPersonIndexed(y))));
        }
    }
}
