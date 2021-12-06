using System;
using System.Threading.Tasks;
using HousingSearchListener.V1.Domain.Transaction;

namespace HousingSearchListener.V1.Gateway
{
    public interface IFinancialTransactionApiGateway
    {
        Task<TransactionResponseObject> GetTransactionByIdAsync(Guid id, Guid targetId, Guid correlationId);
    }
}
