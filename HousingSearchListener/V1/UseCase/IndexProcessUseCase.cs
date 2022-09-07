using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Gateway.Interfaces;
using Hackney.Shared.HousingSearch.Domain.Process;
using System.Collections.Generic;

namespace HousingSearchListener.V1.UseCase
{
    public class IndexProcessUseCase : IIndexProcessUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IProcessesApiGateway _processesApiGateway;
        private readonly ITenureApiGateway _tenureApiGateway;
        private readonly IPersonApiGateway _personApiGateway;
        private readonly IAssetApiGateway _assetApiGateway;
        private readonly IESEntityFactory _esProcessesFactory;

        public IndexProcessUseCase(IEsGateway esGateway,
                                   IProcessesApiGateway processesApiGateway,
                                   ITenureApiGateway tenureApiGateway,
                                   IPersonApiGateway personApiGateway,
                                   IAssetApiGateway assetApiGateway,
                                   IESEntityFactory esProcessesFactory)
        {
            _esGateway = esGateway;
            _processesApiGateway = processesApiGateway;
            _tenureApiGateway = tenureApiGateway;
            _personApiGateway = personApiGateway;
            _assetApiGateway = assetApiGateway;
            _esProcessesFactory = esProcessesFactory;
        }


        [LogCall]
        private async Task<RelatedEntity> GetTargetRelatedEntity(Process process, Guid correlationId)
        {
            switch (process.TargetType)
            {
                case TargetType.tenure:
                    var tenure = await _tenureApiGateway.GetTenureByIdAsync(process.TargetId, correlationId)
                                                        .ConfigureAwait(false);
                    return new RelatedEntity
                    {
                        Id = Guid.Parse(tenure.Id),
                        TargetType = TargetType.tenure
                    };
                case TargetType.person:
                    var person = await _personApiGateway.GetPersonByIdAsync(process.TargetId, correlationId).ConfigureAwait(false);
                    return new RelatedEntity
                    {
                        Id = Guid.Parse(person.Id),
                        TargetType = TargetType.person,
                        Description = person.FullName
                    };
                case TargetType.asset:
                    var asset = await _assetApiGateway.GetAssetByIdAsync(process.TargetId, correlationId).ConfigureAwait(false);
                    var assetAddress = asset.AssetAddress;
                    var fullAddress = $"{assetAddress.AddressLine1} {assetAddress.AddressLine2} {assetAddress.AddressLine3} {assetAddress.AddressLine4} {assetAddress.PostCode}";
                    return new RelatedEntity
                    {
                        Id = asset.Id,
                        TargetType = TargetType.asset,
                        Description = fullAddress
                    };
                default:
                    throw new Exception($"Unknown target type: {process.TargetType}");
            }
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get process from Processes  API
            var process = await _processesApiGateway.GetProcessesByIdAsync(message.EntityId, message.CorrelationId)
                                                    .ConfigureAwait(false);
            if (process is null) throw new EntityNotFoundException<Process>(message.EntityId);

            // 2. Get target entity from relevant API
            var targetRelatedEntity = await GetTargetRelatedEntity(process, message.CorrelationId).ConfigureAwait(false);

            if (process.RelatedEntities is null)
                process.RelatedEntities = new List<RelatedEntity> { targetRelatedEntity };
            else process.RelatedEntities.Add(targetRelatedEntity);

            // 3. Update the ES index
            var esProcess = _esProcessesFactory.CreateProcess(process);
            await _esGateway.IndexProcess(esProcess);
        }
    }
}