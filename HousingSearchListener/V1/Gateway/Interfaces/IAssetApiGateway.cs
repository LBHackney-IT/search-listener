using Hackney.Shared.Asset.Domain;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway.Interfaces
{
    public interface IAssetApiGateway
    {
        Task<Hackney.Shared.HousingSearch.Domain.Asset.Asset> GetAssetByIdAsync(Guid entityId, Guid correlationId);
    }
}