using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Gateway.Interfaces;
using System.Collections.Generic;
using Hackney.Shared.Tenure.Domain;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Domain.Person;
using HousingSearchApi.V1.Factories;
using HousingSearchListener.V1.UseCase.Exceptions;
using Hackney.Shared.Processes.Domain;
using HousingSearchProcess = Hackney.Shared.HousingSearch.Domain.Process.Process;
using Hackney.Shared.HousingSearch.Factories;
using Process = Hackney.Shared.Processes.Domain.Process;
using Hackney.Shared.Processes.Factories;

namespace HousingSearchListener.V1.UseCase
{
    public class IndexProcessUseCase : IIndexProcessUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly ITenureApiGateway _tenureApiGateway;
        private readonly IPersonApiGateway _personApiGateway;
        private readonly IAssetApiGateway _assetApiGateway;
        private readonly IESEntityFactory _esProcessesFactory;

        public IndexProcessUseCase(IEsGateway esGateway,
                                   ITenureApiGateway tenureApiGateway,
                                   IPersonApiGateway personApiGateway,
                                   IAssetApiGateway assetApiGateway,
                                   IESEntityFactory esProcessesFactory)
        {
            _esGateway = esGateway;
            _tenureApiGateway = tenureApiGateway;
            _personApiGateway = personApiGateway;
            _assetApiGateway = assetApiGateway;
            _esProcessesFactory = esProcessesFactory;
        }


        [LogCall]
        private async Task<RelatedEntity> GetTargetRelatedEntity(Process process, Guid correlationId)
        {
            var targetId = process.TargetId;
            switch (process.TargetType)
            {
                case TargetType.tenure:
                    var tenure = await _tenureApiGateway.GetTenureByIdAsync(targetId, correlationId)
                                                        .ConfigureAwait(false);
                    if (tenure is null) throw new EntityNotFoundException<TenureInformation>(targetId);

                    return new RelatedEntity
                    {
                        Id = Guid.Parse(tenure.Id),
                        TargetType = TargetType.tenure
                    };
                case TargetType.person:
                    var person = await _personApiGateway.GetPersonByIdAsync(targetId, correlationId).ConfigureAwait(false);
                    if (person is null) throw new EntityNotFoundException<Person>(targetId);

                    return new RelatedEntity
                    {
                        Id = Guid.Parse(person.Id),
                        TargetType = TargetType.person,
                        Description = person.FullName
                    };
                case TargetType.asset:
                    var asset = await _assetApiGateway.GetAssetByIdAsync(targetId, correlationId).ConfigureAwait(false);
                    if (asset is null) throw new EntityNotFoundException<Hackney.Shared.HousingSearch.Domain.Asset.Asset>(targetId);

                    var assetAddress = asset.AssetAddress;
                    var fullAddress = $"{assetAddress.AddressLine1} {assetAddress.AddressLine2} {assetAddress.AddressLine3} {assetAddress.AddressLine4} {assetAddress.PostCode}";
                    return new RelatedEntity
                    {
                        Id = Guid.Parse(asset.Id),
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

            // 1. Get process from message
            var process = GetProcessFromEventData(message.EventData.NewData);
            if (process is null) throw new InvalidEventDataTypeException<Process>(message.Id);

            // 2. Get target entity from relevant API if necessary
            process.RelatedEntities = process.RelatedEntities ?? new List<RelatedEntity>();
            if (!process.RelatedEntities.Exists(x => x.Id == process.TargetId))
            {
                var targetRelatedEntity = await GetTargetRelatedEntity(process, message.CorrelationId).ConfigureAwait(false);
                process.RelatedEntities.Add(targetRelatedEntity);
            }

            // 3. Update the ES index
            var esProcess = process.ToElasticSearch();
            await _esGateway.IndexProcess(esProcess);
        }

        private static Process GetProcessFromEventData(object data)
        {
            return (data is Process) ? data as Process : ObjectFactory.ConvertFromObject<Process>(data);
        }
    }
}