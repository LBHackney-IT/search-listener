using AutoFixture;
using FluentAssertions;
using Force.DeepCloner;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
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

namespace HousingSearchListener.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class AddPersonToTenureUseCaseTests
    {
        private readonly Mock<ITenureApiGateway> _mockTenureApi;
        private readonly Mock<IPersonApiGateway> _mockPersonApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly AddPersonToTenureUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly TenureInformation _tenure;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();
        private const string DateFormat = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";

        public AddPersonToTenureUseCaseTests()
        {
            _fixture = new Fixture();

            _mockPersonApi = new Mock<IPersonApiGateway>();
            _mockTenureApi = new Mock<ITenureApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _esEntityFactory = new ESEntityFactory();
            _sut = new AddPersonToTenureUseCase(_mockEsGateway.Object,
                _mockTenureApi.Object, _mockPersonApi.Object, _esEntityFactory);

            _tenure = CreateTenure();
            _message = CreateMessage(Guid.Parse(_tenure.Id));

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);
        }

        private EntityEventSns CreateMessage(Guid tenureId, string eventType = EventTypes.PersonAddedToTenureEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.EntityId, tenureId)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private TenureInformation CreateTenure()
        {
            return _fixture.Build<TenureInformation>()
                           .With(x => x.Id, Guid.NewGuid().ToString())
                           .With(x => x.StartOfTenureDate, DateTime.UtcNow.AddMonths(-6).ToString(DateFormat))
                           .With(x => x.EndOfTenureDate, DateTime.UtcNow.AddYears(6).ToString(DateFormat))
                           .With(x => x.HouseholdMembers, _fixture.Build<HouseholdMembers>()
                                                                  .With(x => x.Id, Guid.NewGuid().ToString())
                                                                  .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-40).ToString(DateFormat))
                                                                  .With(x => x.PersonTenureType, "Tenant")
                                                                  .CreateMany(3).ToList())
                           .Create();
        }

        private Person CreatePerson(Guid? entityId)
        {
            if (!entityId.HasValue) return null;

            var tenures = _fixture.CreateMany<Tenure>(1).ToList();
            return _fixture.Build<Person>()
                           .With(x => x.Id, entityId.ToString())
                           .With(x => x.Tenures, tenures)
                           .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                           .Create();
        }

        private Guid? SetMessageEventData(TenureInformation tenure, EntityEventSns message, bool hasChanges, HouseholdMembers added = null)
        {
            var oldData = tenure.HouseholdMembers;
            var newData = oldData.DeepClone();
            message.EventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };

            Guid? personId = null;
            if (hasChanges)
            {
                if (added is null)
                {
                    var changed = newData.First();
                    changed.FullName = "Updated name";
                    personId = Guid.Parse(changed.Id);
                }
                else
                {
                    newData.Add(added);
                    personId = Guid.Parse(added.Id);
                }
            }
            return personId;
        }

        private bool VerifyPersonIndexed(QueryablePerson esPerson, Person person, TenureInformation tenure)
        {
            esPerson.Should().BeEquivalentTo(_esEntityFactory.CreatePerson(person),
                                                  c => c.Excluding(x => x.Tenures)
                                                        .Excluding(x => x.PersonTypes));

            var newTenure = esPerson.Tenures.FirstOrDefault(x => x.Id == tenure.Id);
            newTenure.Should().NotBeNull();
            newTenure.AssetFullAddress.Should().Be(tenure.TenuredAsset.FullAddress);
            newTenure.EndDate.Should().Be(tenure.EndOfTenureDate);
            newTenure.StartDate.Should().Be(tenure.StartOfTenureDate);
            newTenure.Type.Should().Be(tenure.TenureType.Description);

            esPerson.PersonTypes.Should().Contain("Tenant");
            return true;
        }

        private bool VerifyTenureIndexed(QueryableTenure esTenure)
        {
            esTenure.Should().BeEquivalentTo(_esEntityFactory.CreateQueryableTenure(_tenure));
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
        public void ProcessMessageAsyncTestGetPersonExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetPersonReturnsNullThrows()
        {
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(It.IsAny<Guid>(), _message.CorrelationId))
                                       .ReturnsAsync((Person)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<Person>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexTenureExceptionThrows()
        {
            var personId = SetMessageEventData(_tenure, _message, true);
            var person = CreatePerson(personId);
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(personId.Value, _message.CorrelationId))
                                       .ReturnsAsync(person);

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);

            var exMsg = "This is the last error";
            _mockEsGateway.Setup(x => x.IndexTenure(It.IsAny<QueryableTenure>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

            _mockEsGateway.Verify(x => x.IndexTenure(It.Is<QueryableTenure>(y => VerifyTenureIndexed(y))), Times.Once);
            _mockEsGateway.Verify(x => x.IndexPerson(It.Is<QueryablePerson>(y => VerifyPersonIndexed(y, person, _tenure))), Times.Never);
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexPersonExceptionThrows()
        {
            var personId = SetMessageEventData(_tenure, _message, true);
            var person = CreatePerson(personId);
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(personId.Value, _message.CorrelationId))
                                       .ReturnsAsync(person);
            var exMsg = "This is the last error";
            _mockEsGateway.Setup(x => x.IndexPerson(It.IsAny<QueryablePerson>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

            _mockEsGateway.Verify(x => x.IndexTenure(It.Is<QueryableTenure>(y => VerifyTenureIndexed(y))), Times.Once);
            _mockEsGateway.Verify(x => x.IndexPerson(It.Is<QueryablePerson>(y => VerifyPersonIndexed(y, person, _tenure))), Times.Once);
        }

        [Theory]
        [InlineData(EventTypes.PersonAddedToTenureEvent, false)]
        [InlineData(EventTypes.PersonAddedToTenureEvent, true)]
        public async Task ProcessMessageAsyncTestIndexBothSuccess(string eventType, bool added)
        {
            _message.EventType = eventType;

            HouseholdMembers newHm = added ?
                _fixture.Build<HouseholdMembers>()
                        .With(x => x.Id, Guid.NewGuid().ToString())
                        .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-40).ToString(DateFormat))
                        .With(x => x.PersonTenureType, "Tenant")
                        .Create()
                : null;

            var personId = SetMessageEventData(_tenure, _message, true, newHm);
            var person = CreatePerson(personId);
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(personId.Value, _message.CorrelationId))
                                       .ReturnsAsync(person);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexTenure(It.Is<QueryableTenure>(y => VerifyTenureIndexed(y))), Times.Once);
            _mockEsGateway.Verify(x => x.IndexPerson(It.Is<QueryablePerson>(y => VerifyPersonIndexed(y, person, _tenure))), Times.Once);
        }
    }
}
