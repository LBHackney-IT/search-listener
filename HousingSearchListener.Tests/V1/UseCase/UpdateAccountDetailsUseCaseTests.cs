using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Account;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Factories.QueryableFactories;
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
    public class UpdateAccountDetailsUseCaseTests
    {
        private readonly Mock<IAccountApiGateway> _mockAccountApi;
        private readonly Mock<ITenureApiGateway> _mockTenureApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly UpdateAccountDetailsUseCase _sut;

        private readonly TenuresFactory _tenureFactory;
        private readonly EntityEventSns _message;
        private readonly AccountResponseObject _account;
        private readonly TenureInformation _tenure;
        private readonly QueryableAsset _esAsset;
        private readonly QueryableTenure _esTenure;
        private readonly List<QueryablePerson> _esPersons;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();
        private const string DateFormat = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";
        private const string TheNewPaymentReference = "some-new-payment-reference";

        public UpdateAccountDetailsUseCaseTests()
        {
            _fixture = new Fixture();
            _tenureFactory = new TenuresFactory();

            _mockAccountApi = new Mock<IAccountApiGateway>();
            _mockTenureApi = new Mock<ITenureApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _sut = new UpdateAccountDetailsUseCase(_mockEsGateway.Object,
                _mockAccountApi.Object, _mockTenureApi.Object);

            _account = CreateAccount();
            _tenure = CreateTenure(_account.TargetId);
            _message = CreateMessage(Guid.Parse(_tenure.Id));

            _esTenure = CreateQueryableTenure(_tenure);
            _esAsset = CreateQueryableAsset(_tenure.TenuredAsset.Id, _tenure.Id);
            _esPersons = CreateQueryablePersons(_tenure);

            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                                        .ReturnsAsync(_account);
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_account.TargetId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);

            _mockEsGateway.Setup(x => x.GetAssetById(_tenure.TenuredAsset.Id)).ReturnsAsync(_esAsset);
            _mockEsGateway.Setup(x => x.GetTenureById(_tenure.Id)).ReturnsAsync(_esTenure);
            foreach (var hm in _tenure.HouseholdMembers)
                _mockEsGateway.Setup(x => x.GetPersonById(hm.Id)).ReturnsAsync(_esPersons.First(y => y.Id == hm.Id));
        }

        private EntityEventSns CreateMessage(Guid accountId, string eventType = EventTypes.AccountCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.EntityId, accountId)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private QueryableAsset CreateQueryableAsset(string id, string tenureId)
        {
            return _fixture.Build<QueryableAsset>()
                           .With(x => x.Id, id)
                           .With(x => x.Tenure, _fixture.Build<QueryableAssetTenure>()
                                                        .With(y => y.Id, tenureId)
                                                        .Create())
                           .Create();
        }

        private QueryableTenure CreateQueryableTenure(TenureInformation tenure)
        {
            return _tenureFactory.CreateQueryableTenure(tenure);
        }

        private List<QueryablePerson> CreateQueryablePersons(TenureInformation tenure)
        {
            return tenure.HouseholdMembers
                         .Select(x => CreateQueryablePerson(x, tenure))
                         .ToList();
        }

        private QueryablePerson CreateQueryablePerson(HouseholdMembers hm, TenureInformation tenure)
        {
            var tenures = _fixture.CreateMany<QueryablePersonTenure>(3).ToList();
            tenures.Last().Id = tenure.Id;
            return _fixture.Build<QueryablePerson>()
                           .With(x => x.Id, hm.Id)
                           .With(x => x.Tenures, tenures)
                           .Create();
        }

        private AccountResponseObject CreateAccount()
        {
            return _fixture.Build<AccountResponseObject>()
                           .With(x => x.StartDate, DateTime.UtcNow.AddMonths(-6).ToString(DateFormat))
                           .With(x => x.EndDate, (string)null)
                           .With(x => x.PaymentReference, TheNewPaymentReference)
                           .Create();
        }

        private TenureInformation CreateTenure(Guid id)
        {
            return _fixture.Build<TenureInformation>()
                           .With(x => x.Id, id.ToString())
                           .With(x => x.StartOfTenureDate, DateTime.UtcNow.AddMonths(-6).ToString(DateFormat))
                           .With(x => x.EndOfTenureDate, DateTime.UtcNow.AddYears(6).ToString(DateFormat))
                           .With(x => x.HouseholdMembers, _fixture.Build<HouseholdMembers>()
                                                                  .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-40).ToString(DateFormat))
                                                                  .With(x => x.PersonTenureType, "Tenant")
                                                                  .CreateMany(3).ToList())
                           .Create();
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetAccountExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                                        .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetAccountReturnsNullThrows()
        {
            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                                        .ReturnsAsync((AccountResponseObject)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<AccountResponseObject>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTenureExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_account.TargetId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTenureReturnsNullThrows()
        {
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_account.TargetId, _message.CorrelationId))
                                       .ReturnsAsync((TenureInformation)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<TenureInformation>>();
        }

        [Fact]
        public async Task ProcessMessageAsyncTestIndexSuccess()
        {
            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.GetAssetById(_esAsset.Id), Times.Once());
            _mockEsGateway.Verify(x => x.GetTenureById(_esTenure.Id), Times.Once());
            _mockEsGateway.Verify(x => x.GetPersonById(It.Is<string>(y => _esPersons.Select(z => z.Id).Contains(y))),
                                  Times.Exactly(_tenure.HouseholdMembers.Count));

            _mockEsGateway.Verify(x => x.IndexAsset(It.Is<QueryableAsset>(y => VerifyAssetIndexed(y, _esAsset))), Times.Once);
            _mockEsGateway.Verify(x => x.IndexTenure(It.Is<QueryableTenure>(y => VerifyTenureIndexed(y, _esTenure))), Times.Once);
            foreach (var esPerson in _esPersons)
                _mockEsGateway.Verify(x => x.IndexPerson(It.Is<QueryablePerson>(y => VerifyPersonIndexed(y, esPerson, _esTenure.Id))), Times.Once);
        }

        private bool VerifyAssetIndexed(QueryableAsset esAsset, QueryableAsset startingAsset)
        {
            esAsset.Should().BeEquivalentTo(startingAsset, c => c.Excluding(y => y.Tenure));
            esAsset.Tenure.Should().BeEquivalentTo(startingAsset.Tenure, c => c.Excluding(y => y.PaymentReference));
            esAsset.Tenure.PaymentReference.Should().Be(TheNewPaymentReference);
            return true;
        }

        private bool VerifyTenureIndexed(QueryableTenure esTenure, QueryableTenure startingTenure)
        {
            esTenure.Should().BeEquivalentTo(startingTenure, c => c.Excluding(y => y.PaymentReference));
            esTenure.PaymentReference.Should().Be(TheNewPaymentReference);
            return true;
        }

        private bool VerifyPersonIndexed(QueryablePerson esPerson, QueryablePerson startingPerson, string tenureId)
        {
            if (esPerson.Id != startingPerson.Id)
                return false;

            esPerson.Should().BeEquivalentTo(startingPerson, c => c.Excluding(y => y.Tenures));
            var personTenure = esPerson.Tenures.First(x => x.Id == tenureId);
            personTenure.PaymentReference.Should().Be(TheNewPaymentReference);
            return true;
        }
    }
}
