using Hackney.Shared.HousingSearch.Domain.Asset;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway.Interfaces
{
    public interface IAssetApiGateway
    {
        Task<Asset> GetAssetByIdAsync(Guid entityId, Guid correlationId);
    }
}