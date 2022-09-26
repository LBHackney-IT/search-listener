using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Process;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;
using HousingSearchApi.V1.Factories;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class UpdateProcessUseCase : IUpdateProcessUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IESEntityFactory _esProcessesFactory;

        public UpdateProcessUseCase(IEsGateway esGateway,
                                   IESEntityFactory esProcessesFactory)
        {
            _esGateway = esGateway;
            _esProcessesFactory = esProcessesFactory;
        }


        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get new state from message
            var newState = GetStateChangeDataFromEventData(message.EventData.NewData);
            if (newState is null) throw new InvalidEventDataTypeException<ProcessStateChangeData>(message.Id);
            // 2. Update the ES index
            var esProcess = await _esGateway.GetProcessById(message.EntityId.ToString()).ConfigureAwait(false);
            if (esProcess is null) throw new EntityNotIndexedException<QueryableProcess>(message.EntityId.ToString());

            esProcess.State = newState.State;

            await _esGateway.IndexProcess(esProcess);
        }

        private static ProcessStateChangeData GetStateChangeDataFromEventData(object data)
        {
            return (data is ProcessStateChangeData) ? data as ProcessStateChangeData : ObjectFactory.ConvertFromObject<ProcessStateChangeData>(data);
        }
    }
}
