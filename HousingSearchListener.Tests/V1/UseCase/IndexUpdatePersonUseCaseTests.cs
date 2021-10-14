using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase;
using HousingSearchListener.V1.UseCase.Interfaces;
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
    public class IndexUpdatePersonUseCaseTests
    {
        private readonly Mock<ITenureApiGateway> _mockTenureApi;
        private readonly Mock<IPersonApiGateway> _mockPersonApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly Mock<IIndexCreatePersonUseCase> _mockCreatePersonUseCase;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly IndexUpdatePersonUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly TenureInformation _tenure;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public IndexUpdatePersonUseCaseTests()
        {
            _fixture = new Fixture();

            _mockTenureApi = new Mock<ITenureApiGateway>();
            _mockPersonApi = new Mock<IPersonApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _mockCreatePersonUseCase = new Mock<IIndexCreatePersonUseCase>();
            _esEntityFactory = new ESEntityFactory();

            _sut = new IndexUpdatePersonUseCase(_mockEsGateway.Object, _mockPersonApi.Object,
                _mockTenureApi.Object, _esEntityFactory);

            _message = CreateMessage();
            _tenure = CreateTenure(_message.EntityId);
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
            func.Should().ThrowAsync<EntityNotFoundException<Tenure>>();
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
        public async Task ProcessMessageAsyncTestIndexTenureSuccess(string eventType)
        {
            _message.EventType = eventType;

            var mockPerson = _fixture.Build<Person>().Create();
            mockPerson.Tenures = new List<Tenure>(new[] { mockPerson.Tenures[0] });
            mockPerson.Tenures[0].Id = Guid.NewGuid().ToString();

            _tenure.Id = mockPerson.Tenures[0].Id;
            _tenure.HouseholdMembers.Last().Id = mockPerson.Id;
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(Guid.Parse(_tenure.Id), _message.CorrelationId))
                .ReturnsAsync(_tenure);

            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(It.IsAny<Guid>(), _message.CorrelationId)).ReturnsAsync(mockPerson);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockEsGateway.Verify(x => x.IndexTenure(It.Is<QueryableTenure>(y => VerifyTenureIndexed(y))), Times.Once);
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.PersonCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                .With(x => x.EventType, eventType)
                .With(x => x.CorrelationId, _correlationId)
                .Create();
        }

        private TenureInformation CreateTenure(Guid entityId)
        {
            var tenures = _fixture.CreateMany<Tenure>(1).ToList();
            return _fixture.Build<TenureInformation>()
                .With(x => x.Id, entityId.ToString())
                .Create();
        }

        private bool VerifyTenureIndexed(QueryableTenure esTenure)
        {
            esTenure.Should().BeEquivalentTo(_esEntityFactory.CreateQueryableTenure(_tenure));
            return true;
        }
    }
}
