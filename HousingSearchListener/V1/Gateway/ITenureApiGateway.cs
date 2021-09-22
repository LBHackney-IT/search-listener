using HousingSearchListener.V1.Domain.Tenure;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public interface ITenureApiGateway
    {
        Task<TenureInformation> GetTenureByIdAsync(Guid id, Guid correlationId);
    }
}
