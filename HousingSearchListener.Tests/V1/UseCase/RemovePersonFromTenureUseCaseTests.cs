﻿using AutoFixture;
using FluentAssertions;
using Force.DeepCloner;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories.QueryableFactories;
using HousingSearchListener.V1.Gateway.Interfaces;
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
    public class RemovePersonFromTenureUseCaseTests
    {
        private readonly Mock<ITenureApiGateway> _mockTenureApi;
        private readonly Mock<IPersonApiGateway> _mockPersonApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly PersonFactory _personFactory;
        private readonly TenuresFactory _tenuresFactory;
        private readonly RemovePersonFromTenureUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly TenureInformation _tenure;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();
        private const string DateFormat = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";

        public RemovePersonFromTenureUseCaseTests()
        {
            _fixture = new Fixture();

            _mockPersonApi = new Mock<IPersonApiGateway>();
            _mockTenureApi = new Mock<ITenureApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _personFactory = new PersonFactory();
            _tenuresFactory = new TenuresFactory();
            _sut = new RemovePersonFromTenureUseCase(_mockEsGateway.Object,
                _mockTenureApi.Object, _mockPersonApi.Object, _personFactory, _tenuresFactory);

            _tenure = CreateTenure();
            _message = CreateMessage(Guid.Parse(_tenure.Id));

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);
        }

        private EntityEventSns CreateMessage(Guid tenureId, string eventType = EventTypes.PersonRemovedFromTenureEvent)
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
                                                                  .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-40).ToString(DateFormat))
                                                                  .With(x => x.PersonTenureType, "Tenant")
                                                                  .CreateMany(3).ToList())
                           .Create();
        }

        private QueryableTenureType ToQueryable(TenureType tt)
        {
            return new QueryableTenureType { Code = tt.Code, Description = tt.Description };
        }

        private QueryableTenure CreateQueryableTenureForPerson(string tenureId, string personId, string personType)
        {
            QueryableTenureType tt;
            bool isResponsible;
            switch (personType)
            {
                case "HouseholderMember":
                    tt = ToQueryable(TenureTypes.Secure);
                    isResponsible = false;
                    break;
                case "Freeholder":
                    tt = ToQueryable(TenureTypes.Freehold);
                    isResponsible = true;
                    break;
                default:
                    tt = ToQueryable(TenureTypes.Secure);
                    isResponsible = true;
                    break;
            }
            var hms = _fixture.Build<QueryableHouseholdMember>()
                              .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-40).ToString(DateFormat))
                              .With(x => x.PersonTenureType, personType)
                              .With(x => x.IsResponsible, isResponsible)
                              .CreateMany(3).ToList();
            hms.Last().Id = personId;

            return _fixture.Build<QueryableTenure>()
                           .With(x => x.Id, tenureId)
                           .With(x => x.TenureType, tt)
                           .With(x => x.HouseholdMembers, hms)
                           .Create();
        }

        private Person CreatePerson(Guid? entityId, bool hasThisTenure = true)
        {
            if (!entityId.HasValue) return null;

            var tenures = _fixture.CreateMany<Tenure>(3).ToList();
            if (hasThisTenure)
                tenures.Last().Id = _tenure.Id;
            var personTypes = new List<string> { "Tenant", "HouseholderMember", "Freeholder" };
            var person = _fixture.Build<Person>()
                           .With(x => x.Id, entityId.ToString())
                           .With(x => x.Tenures, tenures)
                           .With(x => x.PersonTypes, personTypes)
                           .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                           .Create();

            for (int i = 0; i < tenures.Count; i++)
            {
                var personTenure = tenures[i];
                var personType = personTypes[i];
                var t = CreateQueryableTenureForPerson(personTenure.Id, person.Id, personType);
                _mockEsGateway.Setup(x => x.GetTenureById(personTenure.Id)).ReturnsAsync(t);
            }

            return person;
        }

        private Guid SetMessageEventData(TenureInformation tenure, EntityEventSns message)
        {
            var newData = tenure.HouseholdMembers;
            var oldData = newData.DeepClone();
            message.EventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };

            var removedHm = _fixture.Build<HouseholdMembers>()
                                    .With(x => x.Id, Guid.NewGuid().ToString())
                                    .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-40).ToString(DateFormat))
                                    .With(x => x.PersonTenureType, "Tenant")
                                    .Create();
            var personId = Guid.Parse(removedHm.Id);
            oldData.Add(removedHm);
            return personId;
        }

        private bool VerifyPersonIndexed(QueryablePerson esPerson, Person startingPerson)
        {
            esPerson.Should().BeEquivalentTo(_personFactory.CreatePerson(startingPerson),
                                             c => c.Excluding(y => y.Tenures).Excluding(y => y.PersonTypes));
            esPerson.Tenures.Should().NotContain(x => x.Id == _tenure.Id);
            esPerson.PersonTypes.Should().HaveCount(2);
            esPerson.PersonTypes.Should().NotContain("Freeholder");
            return true;
        }

        private bool VerifyTenureIndexed(QueryableTenure esTenure)
        {
            esTenure.Should().BeEquivalentTo(_tenuresFactory.CreateQueryableTenure(_tenure));
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
            var personId = SetMessageEventData(_tenure, _message);

            var exMsg = "This is an error";
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(personId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetPersonReturnsNullThrows()
        {
            var personId = SetMessageEventData(_tenure, _message);
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(personId, _message.CorrelationId))
                                       .ReturnsAsync((Person)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<Person>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexTenureExceptionThrows()
        {
            var personId = SetMessageEventData(_tenure, _message);
            var person = CreatePerson(personId);
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(personId, _message.CorrelationId))
                                       .ReturnsAsync(person);

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);

            var exMsg = "This is the last error";
            _mockEsGateway.Setup(x => x.IndexTenure(It.IsAny<QueryableTenure>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

            _mockEsGateway.Verify(x => x.IndexTenure(It.Is<QueryableTenure>(y => VerifyTenureIndexed(y))), Times.Once);
            _mockEsGateway.Verify(x => x.IndexPerson(It.Is<QueryablePerson>(y => VerifyPersonIndexed(y, person))), Times.Never);
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexPersonExceptionThrows()
        {
            var personId = SetMessageEventData(_tenure, _message);
            var person = CreatePerson(personId);
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(personId, _message.CorrelationId))
                                       .ReturnsAsync(person);
            var exMsg = "This is the last error";
            _mockEsGateway.Setup(x => x.IndexPerson(It.IsAny<QueryablePerson>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

            _mockEsGateway.Verify(x => x.IndexTenure(It.Is<QueryableTenure>(y => VerifyTenureIndexed(y))), Times.Once);
            _mockEsGateway.Verify(x => x.IndexPerson(It.Is<QueryablePerson>(y => VerifyPersonIndexed(y, person))), Times.Once);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestIndexBothSuccess()
        {
            var personId = SetMessageEventData(_tenure, _message);

            var person = CreatePerson(personId);
            var startingPerson = person.DeepClone();
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(personId, _message.CorrelationId))
                                       .ReturnsAsync(person);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.GetTenureById(It.IsAny<string>()), Times.Exactly(startingPerson.Tenures.Count - 1));
            _mockEsGateway.Verify(x => x.IndexTenure(It.Is<QueryableTenure>(y => VerifyTenureIndexed(y))), Times.Once);
            _mockEsGateway.Verify(x => x.IndexPerson(It.Is<QueryablePerson>(y => VerifyPersonIndexed(y, startingPerson))), Times.Once);
        }
    }
}
