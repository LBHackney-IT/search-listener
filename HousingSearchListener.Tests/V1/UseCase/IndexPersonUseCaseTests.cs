using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class IndexPersonUseCaseTests
    {
        private readonly Mock<IPersonApiGateway> _mockPersonApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly IndexPersonUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly Person _person;

        private readonly Fixture _fixture;

        public IndexPersonUseCaseTests()
        {
            _fixture = new Fixture();

            _mockPersonApi = new Mock<IPersonApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _esEntityFactory = new ESEntityFactory();
            _sut = new IndexPersonUseCase(_mockEsGateway.Object,
                _mockPersonApi.Object, _esEntityFactory);

            _message = CreateMessage();
            _person = CreatePerson(_message.EntityId);
        }

        private EntityEventSns CreateMessage(EventTypes eventType = EventTypes.PersonCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType.ToString())
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

        private bool VerifyPersonIndexed(ESPerson esPerson)
        {
            esPerson.Should().BeEquivalentTo(_esEntityFactory.CreatePerson(_person));
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
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetPersonReturnsNullThrows()
        {
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId))
                                       .ReturnsAsync((Person)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<Person>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexPersonExceptionThrows()
        {
            var exMsg = "This is the last error";
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId))
                                       .ReturnsAsync(_person);
            _mockEsGateway.Setup(x => x.IndexPerson(It.IsAny<ESPerson>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Theory]
        [InlineData(EventTypes.PersonCreatedEvent)]
        [InlineData(EventTypes.PersonUpdatedEvent)]
        public async Task ProcessMessageAsyncTestIndexPersonSuccess(EventTypes eventType)
        {
            _message.EventType = eventType.ToString();

            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId))
                                       .ReturnsAsync(_person);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexPerson(It.Is<ESPerson>(y => VerifyPersonIndexed(y))), Times.Once);
        }
    }
}
