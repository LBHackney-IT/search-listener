using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;
using HousingSearchListener.V1.Domain.Transaction;

namespace HousingSearchListener.V1.Factories.Interfaces
{
    public interface ITransactionFactory
    {
        QueryableTransaction CreateQueryableTransaction(TransactionResponseObject transaction);
    }
}
