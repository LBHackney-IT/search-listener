using System;
using System.Threading.Tasks;
using Hackney.Shared.HousingSearch.Domain.Process;

namespace HousingSearchListener.V1.Gateway.Interfaces
{
    public interface IProcessesApiGateway
    {
        Task<Process> GetProcessByIdAsync(Guid entityId, Guid correlationId);
    }
}