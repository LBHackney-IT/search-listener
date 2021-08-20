using HousingSearchListener.V1.Boundary;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase.Interfaces
{
    public interface IMessageProcessing
    {
        Task ProcessMessageAsync(EntityEventSns message);
    }
}
