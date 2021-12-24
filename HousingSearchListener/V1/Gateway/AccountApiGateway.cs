using Hackney.Core.Logging;
using HousingSearchListener.V1.Boundary.Response;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class AccountApiGateway : IAccountApiGateway
    {
        private const string ApiName = "Account";
        private const string AccountApiUrl = "AccountApiUrl";
        private const string AccountApiToken = "AccountApiToken";

        private readonly IApiGateway _apiGateway;

        public AccountApiGateway(IApiGateway apiGateway)
        {
            _apiGateway = apiGateway;
            _apiGateway.Initialise(ApiName, AccountApiUrl, AccountApiToken);
        }

        [LogCall]
        public async Task<AccountResponse> GetAccountByIdAsync(Guid id, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/accounts/{id}";
            return await _apiGateway.GetByIdAsync<AccountResponse>(route, id, correlationId);
        }
    }
}
