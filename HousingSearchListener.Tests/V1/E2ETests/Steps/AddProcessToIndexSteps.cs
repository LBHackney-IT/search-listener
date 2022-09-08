using FluentAssertions;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Domain.Person;
using Hackney.Shared.HousingSearch.Domain.Process;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;
using Hackney.Shared.Tenure.Domain;
using HousingSearchListener.Tests.V1.E2ETests.Fixtures;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class AddProcessToIndexSteps : BaseSteps
    {
        private readonly ESEntityFactory _entityFactory = new ESEntityFactory();

        public AddProcessToIndexSteps()
        {
            _eventType = EventTypes.ProcessStartedEvent;
        }

        public async Task WhenTheFunctionIsTriggered(Guid ProcessId, string eventType)
        {
            var eventMsg = CreateEvent(ProcessId, eventType);
            await TriggerFunction(CreateMessage(eventMsg));
        }

        public void ThenTheCorrelationIdWasUsedInTheApiCalls(ProcessesApiFixture processesApiFixture,
                                                             AssetApiFixture assetApiFixture,
                                                             PersonApiFixture personApiFixture,
                                                             TenureApiFixture tenureApiFixture)
        {
            var receivedCorrelationIds = processesApiFixture.ReceivedCorrelationIds.Concat(assetApiFixture.ReceivedCorrelationIds)
                                                                                   .Concat(personApiFixture.ReceivedCorrelationIds)
                                                                                   .Concat(tenureApiFixture.ReceivedCorrelationIds)
                                                                                   .ToList();
            receivedCorrelationIds.Where(x => x == _correlationId.ToString()).Should().HaveCount(2);
        }

        public void ThenAnEntityNotFoundExceptionIsThrown<T>(Guid id) where T : class
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<T>));
            (_lastException as EntityNotFoundException<T>).Id.Should().Be(id);
        }

        public void ThenAProcessNotFoundExceptionIsThrown(Guid id)
        {
            ThenAnEntityNotFoundExceptionIsThrown<Process>(id);
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
                    ThenAnEntityNotFoundExceptionIsThrown<Asset>(id);
                    break;
            }
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

        public async Task ThenTheIndexIsUpdatedWithTheProcess(Process process, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableProcess>(process.Id, g => g.Index("processes"))
                                       .ConfigureAwait(false);
            var processInIndex = result.Source;
            processInIndex.Should().NotBeNull();

            var expectedProcess = _entityFactory.CreateProcess(process);
            processInIndex.Should().BeEquivalentTo(processInIndex, c => c.Excluding(x => x.RelatedEntities));

            processInIndex.RelatedEntities.Should().ContainSingle(x => x.Id == process.TargetId.ToString());
            processInIndex.RelatedEntities.RemoveAll(x => x.Id == process.TargetId.ToString());

            foreach (var relatedEntity in processInIndex.RelatedEntities)
            {
                var processRelatedEntity = expectedProcess.RelatedEntities.Find(x => x.Id.ToString() == relatedEntity.Id);
                processRelatedEntity.Should().NotBeNull();

                relatedEntity.TargetType.Should().BeEquivalentTo(processRelatedEntity.TargetType);
                relatedEntity.SubType.Should().BeEquivalentTo(processRelatedEntity.SubType);
                relatedEntity.Description.Should().BeEquivalentTo(processRelatedEntity.Description);
            }

        }
    }
}
