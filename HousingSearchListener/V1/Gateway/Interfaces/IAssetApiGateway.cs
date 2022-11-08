using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway.Interfaces
{
    public interface IAssetApiGateway
    {
        Task<QueryableAsset> GetAssetByIdAsync(Guid entityId, Guid correlationId);
    }
}