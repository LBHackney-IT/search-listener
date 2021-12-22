using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Factories.QueryableFactories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class IndexCreatePersonUseCaseTests
    {
        private readonly Mock<IPersonApiGateway> _mockPersonApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly PersonFactory _personFactory;
        private readonly IndexCreatePersonUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly Person _person;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public IndexCreatePersonUseCaseTests()
        {
            _fixture = new Fixture();

            _mockPersonApi = new Mock<IPersonApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _personFactory = new PersonFactory();
            _sut = new IndexCreatePersonUseCase(_mockEsGateway.Object,
                _mockPersonApi.Object, _personFactory);

            _message = CreateMessage();
            _person = CreatePerson(_message.EntityId);
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.PersonCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private Person CreatePerson(Guid entityId)
        {
            var tenures = _fixture.CreateMany<Tenure>(1).ToList();
            return _fixture.Build<Person>()
                           .With(x => x.Id, entityId.ToString())
                           .With(x => x.Tenures, tenures)
                           .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                           .Create();
        }

        private bool VerifyPersonIndexed(QueryablePerson esPerson)
        {
            esPerson.Should().BeEquivalentTo(_personFactory.CreatePerson(_person));
            return true;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
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
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync((Person)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<Person>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexPersonExceptionThrows()
        {
            var exMsg = "This is the last error";
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_person);
            _mockEsGateway.Setup(x => x.IndexPerson(It.IsAny<QueryablePerson>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Theory]
        [InlineData(EventTypes.PersonCreatedEvent)]
        public async Task ProcessMessageAsyncTestIndexPersonSuccess(string eventType)
        {
            _message.EventType = eventType;

            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_person);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexPerson(It.Is<QueryablePerson>(y => VerifyPersonIndexed(y))), Times.Once);
        }
    }
}
