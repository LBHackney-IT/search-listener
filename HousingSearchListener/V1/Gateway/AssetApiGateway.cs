using Hackney.Core.Http;
using Hackney.Core.Logging;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using HousingSearchListener.V1.Gateway.Interfaces;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class AssetApiGateway : IAssetApiGateway
    {
        private const string ApiName = "AssetInformationApi";
        private const string AssetApiUrl = "AssetApiUrl";
        private const string AssetApiToken = "AssetApiToken";

        private readonly IApiGateway _apiGateway;

        public AssetApiGateway(IApiGateway apiGateway)
        {
            _apiGateway = apiGateway;
            _apiGateway.Initialise(ApiName, AssetApiUrl, AssetApiToken);
        }

        [LogCall]
        public async Task<QueryableAsset> GetAssetByIdAsync(Guid id, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/assets/{id}";
            return await _apiGateway.GetByIdAsync<QueryableAsset>(route, id, correlationId);
        }
    }
}
