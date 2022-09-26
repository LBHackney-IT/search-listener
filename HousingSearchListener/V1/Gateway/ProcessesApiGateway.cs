using Hackney.Core.Http;
using Hackney.Core.Logging;
using Hackney.Shared.HousingSearch.Domain.Process;
using HousingSearchListener.V1.Gateway.Interfaces;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class ProcessesApiGateway : IProcessesApiGateway
    {
        private const string ApiName = "Processes";
        private const string ProcessesApiUrl = "ProcessesApiUrl";
        private const string ProcessesApiToken = "ProcessesApiToken";

        private readonly IApiGateway _apiGateway;

        public ProcessesApiGateway(IApiGateway apiGateway)
        {
            _apiGateway = apiGateway;
            _apiGateway.Initialise(ApiName, ProcessesApiUrl, ProcessesApiToken);
        }

        [LogCall]
        public async Task<Process> GetProcessByIdAsync(Guid id, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/process/{id}";
            return await _apiGateway.GetByIdAsync<Process>(route, id, correlationId);
        }
    }
}
