using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Process;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class UpdateProcessUseCase : IUpdateProcessUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IProcessesApiGateway _processesApiGateway;
        private readonly IESEntityFactory _esProcessesFactory;

        public UpdateProcessUseCase(IEsGateway esGateway,
                                   IProcessesApiGateway processesApiGateway,
                                   IESEntityFactory esProcessesFactory)
        {
            _esGateway = esGateway;
            _processesApiGateway = processesApiGateway;
            _esProcessesFactory = esProcessesFactory;
        }


        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get process from Processes  API
            var process = await _processesApiGateway.GetProcessByIdAsync(message.EntityId, message.CorrelationId)
                                                    .ConfigureAwait(false);
            if (process is null) throw new EntityNotFoundException<Process>(message.EntityId);

            // 2. Update the ES index
            var esProcess = await _esGateway.GetProcessById(process.Id.ToString()).ConfigureAwait(false);
            esProcess = _esProcessesFactory.CreateProcess(process);
            await _esGateway.IndexProcess(esProcess);
        }
    }
}
