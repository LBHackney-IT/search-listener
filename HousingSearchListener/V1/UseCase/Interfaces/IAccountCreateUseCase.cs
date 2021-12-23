using HousingSearchListener.V1.Boundary;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase.Interfaces
{
    interface IAccountCreateUseCase : IMessageProcessing
    {
        Task ProcessMessageAsync(AccountSnsModel message);
    }
}
