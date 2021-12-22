using HousingSearchListener.V1.Boundary;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase.Interfaces
{
    interface IAccountCreateUseCase
    {
        Task ProcessMessageAsync(AccountSnsModel message);
    }
}
