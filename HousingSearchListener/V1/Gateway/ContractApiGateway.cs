using Hackney.Core.Http;
using Hackney.Core.Logging;
using Hackney.Shared.HousingSearch.Domain.Contract;
using HousingSearchListener.V1.Gateway.Interfaces;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class ContractApiGateway : IContractApiGateway
    {
        private const string ApiName = "ContractApi";
        private const string AssetApiUrl = "ContractApiUrl";
        private const string AssetApiToken = "ContractApiToken";

        private readonly IApiGateway _apiGateway;

        public ContractApiGateway(IApiGateway apiGateway)
        {
            _apiGateway = apiGateway;
            _apiGateway.Initialise(ApiName, AssetApiUrl, AssetApiToken);
        }

        [LogCall]
        public async Task<Contract> GetContractByIdAsync(Guid id, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/contracts/{id}";
            return await _apiGateway.GetByIdAsync<Contract>(route, id, correlationId);
        }
    }
}
