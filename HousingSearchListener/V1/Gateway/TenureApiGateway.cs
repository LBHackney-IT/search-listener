using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.Tenure;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class TenureApiGateway : ITenureApiGateway
    {
        private const string ApiName = "Tenure";
        private const string TenureApiUrl = "TenureApiUrl";
        private const string TenureApiToken = "TenureApiToken";

        private readonly IApiGateway _apiGateway;

        public TenureApiGateway(IApiGateway apiGateway)
        {
            _apiGateway = apiGateway;
            _apiGateway.Initialise(ApiName, TenureApiUrl, TenureApiToken);
        }

        [LogCall]
        public async Task<TenureInformation> GetTenureByIdAsync(Guid id, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/tenures/{id}";
            return await _apiGateway.GetByIdAsync<TenureInformation>(route, id, correlationId);
        }
    }
}
