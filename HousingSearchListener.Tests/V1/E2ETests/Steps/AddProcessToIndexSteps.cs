using AutoFixture;
using FluentAssertions;
using Hackney.Shared.HousingSearch.Domain.Person;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;
using Hackney.Shared.Processes.Domain;
using Hackney.Shared.Processes.Factories;
using Hackney.Shared.Processes.Sns;
using Hackney.Shared.Tenure.Domain;
using HousingSearchListener.Tests.V1.E2ETests.Fixtures;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using ListenerExceptions = HousingSearchListener.V1.UseCase.Exceptions;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;
using Process = Hackney.Shared.Processes.Domain.Process;
using RelatedEntity = Hackney.Shared.Processes.Domain.RelatedEntity;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class AddProcessToIndexSteps : BaseSteps
    {
        private readonly ESEntityFactory _entityFactory = new ESEntityFactory();
        private Process _process;

        public AddProcessToIndexSteps()
        {
            _eventType = EventTypes.ProcessStartedEvent;
        }

        private void SetUpTargetApiFixture(AssetApiFixture assetApiFixture,
                                           PersonApiFixture personApiFixture,
                                           TenureApiFixture tenureApiFixture,
                                           Guid targetId,
                                           TargetType targetType,
                                           bool exists)
        {
            switch (targetType)
            {
                case TargetType.asset:
                    if (exists) assetApiFixture.GivenTheAssetExists(targetId);
                    else assetApiFixture.GivenTheAssetDoesNotExist(targetId);
                    break;
                case TargetType.person:
                    if (exists) personApiFixture.GivenThePersonExists(targetId);
                    else personApiFixture.GivenThePersonDoesNotExist(targetId);
                    break;
                case TargetType.tenure:
                    if (exists) tenureApiFixture.GivenTheTenureExists(targetId);
                    else tenureApiFixture.GivenTheTenureDoesNotExist(targetId);
                    break;
                default:
                    throw new Exception($"Unknown target type: {targetType}");
            }
        }

        public void GivenTheTargetEntityDoesNotExist(AssetApiFixture assetApiFixture,
                                                     PersonApiFixture personApiFixture,
                                                     TenureApiFixture tenureApiFixture,
                                                     Guid targetId,
                                                     TargetType targetType)
        {
            SetUpTargetApiFixture(assetApiFixture, personApiFixture, tenureApiFixture, targetId, targetType, false);
        }

        public void GivenTheTargetEntityExists(AssetApiFixture assetApiFixture,
                                               PersonApiFixture personApiFixture,
                                               TenureApiFixture tenureApiFixture,
                                               Guid targetId,
                                               TargetType targetType)
        {
            SetUpTargetApiFixture(assetApiFixture, personApiFixture, tenureApiFixture, targetId, targetType, true);
        }

        public void GivenTheMessageDoesNotContainAProcess(Guid processId)
        {
            // do nothing
        }

        public void GivenTheMessageContainsAProcess(Guid processId, Guid targetId, TargetType targetType)
        {
            _process = _fixture.Build<Process>()
                               .With(x => x.Id, processId)
                               .With(x => x.TargetId, targetId)
                               .With(x => x.TargetType, targetType)
                               .Create();
        }

        public async Task WhenTheFunctionIsTriggered(Guid ProcessId, string eventType)
        {
            var eventMsg = CreateEvent(ProcessId, eventType);
            eventMsg.EventData.NewData = _process;

            await TriggerFunction(CreateMessage(eventMsg));
        }

        public void ThenAnEntityNotFoundExceptionIsThrown<T>(Guid id) where T : class
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<T>));
            (_lastException as EntityNotFoundException<T>).Id.Should().Be(id);
        }

        public void ThenAnInvalidEventDataTypeExceptionIsThrown<T>() where T : class
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(ListenerExceptions.InvalidEventDataTypeException<T>));
        }

        public void ThenATargetEntityNotFoundExceptionIsThrown(Guid id, TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.tenure:
                    ThenAnEntityNotFoundExceptionIsThrown<TenureInformation>(id);
                    break;
                case TargetType.person:
                    ThenAnEntityNotFoundExceptionIsThrown<Person>(id);
                    break;
                case TargetType.asset:
                    ThenAnEntityNotFoundExceptionIsThrown<QueryableAsset>(id);
                    break;
            }
        }

        public void ThenAProcessStateChangeDataNotFoundExceptionIsThrown(Guid id)
        {
            ThenAnEntityNotFoundExceptionIsThrown<ProcessStateChangeData>(id);
        }

        public async Task ThenTheIndexIsUpdatedWithTheProcess(IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableProcess>(_process.Id, g => g.Index("processes"))
                                       .ConfigureAwait(false);
            var processInIndex = result.Source;
            processInIndex.Should().NotBeNull();

            var expectedProcess = _process.ToDatabase();
            processInIndex.Should().BeEquivalentTo(processInIndex, c => c.Excluding(x => x.RelatedEntities));

            if (_eventType == EventTypes.ProcessStartedEvent)
            {
                processInIndex.RelatedEntities.Should().ContainSingle(x => x.Id == _process.TargetId.ToString());
                processInIndex.RelatedEntities.RemoveAll(x => x.Id == _process.TargetId.ToString());

                foreach (var relatedEntity in processInIndex.RelatedEntities)
                {
                    var processRelatedEntity = expectedProcess.RelatedEntities.Find(x => x.Id == Guid.Parse(relatedEntity.Id));
                    processRelatedEntity.Should().NotBeNull();

                    relatedEntity.TargetType.Should().BeEquivalentTo(processRelatedEntity.TargetType.ToString());
                    relatedEntity.SubType.Should().BeEquivalentTo(processRelatedEntity.SubType?.ToString());
                    relatedEntity.Description.Should().BeEquivalentTo(processRelatedEntity.Description);
                }
            }
        }

        public void ThenNoExceptionsAreThrown()
        {
            _lastException.Should().BeNull();
        }

        public void ThenTheCorrelationIdWasUsedInTheApiCall(AssetApiFixture assetApiFixture, PersonApiFixture personApiFixture, TenureApiFixture tenureApiFixture)
        {
            var receivedCorrelationIds = assetApiFixture.ReceivedCorrelationIds.Concat(personApiFixture.ReceivedCorrelationIds)
                                                                               .Concat(tenureApiFixture.ReceivedCorrelationIds)
                                                                               .ToList();

            receivedCorrelationIds.Select(x => x == _correlationId.ToString()).Should().HaveCount(1);
        }

        public void GivenTheProcessContainsATargetEntity()
        {
            _process.RelatedEntities.Add(_fixture.Build<RelatedEntity>().With(x => x.Id, _process.TargetId).Create());
        }
    }
}
