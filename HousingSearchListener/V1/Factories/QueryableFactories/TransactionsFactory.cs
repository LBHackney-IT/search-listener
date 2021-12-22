using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;
using HousingSearchListener.V1.Domain.Transaction;
using HousingSearchListener.V1.Factories.Interfaces;
using System;

namespace HousingSearchListener.V1.Factories.QueryableFactories
{
    public class TransactionsFactory : ITransactionFactory
    {
        public QueryableTransaction CreateQueryableTransaction(TransactionResponseObject transaction)
        {
            if (transaction is null) throw new ArgumentNullException(nameof(transaction));

            return new QueryableTransaction()
            {
                Id = transaction.Id,
                Address = transaction.Address,
                BalanceAmount = transaction.BalanceAmount,
                BankAccountNumber = transaction.BankAccountNumber,
                ChargedAmount = transaction.ChargedAmount,
                FinancialMonth = transaction.FinancialMonth,
                FinancialYear = transaction.FinancialYear,
                Fund = transaction.Fund,
                HousingBenefitAmount = transaction.HousingBenefitAmount,
                PaidAmount = transaction.PaidAmount,
                PaymentReference = transaction.PaymentReference,
                PeriodNo = transaction.PeriodNo,
                Sender = transaction.Person != null ? new QueryableSender()
                {
                    FullName = transaction.Person.FullName,
                    Id = transaction.Person.Id
                } : null,
                SuspenseResolutionInfo = transaction.SuspenseResolutionInfo != null ? new QueryableSuspenseResolutionInfo()
                {
                    IsApproved = transaction.SuspenseResolutionInfo.IsApproved,
                    IsConfirmed = transaction.SuspenseResolutionInfo.IsConfirmed,
                    Note = transaction.SuspenseResolutionInfo.Note,
                    ResolutionDate = transaction.SuspenseResolutionInfo.ResolutionDate
                } : null,
                TargetId = transaction.TargetId,
                TargetType = transaction.TargetType,
                TransactionAmount = transaction.TransactionAmount,
                TransactionDate = transaction.TransactionDate,
                TransactionSource = transaction.TransactionSource,
                TransactionType = transaction.TransactionType,
                SortCode = transaction.SortCode,
                CreatedAt = transaction.CreatedAt,
                CreatedBy = transaction.CreatedBy,
                LastUpdatedAt = transaction.LastUpdatedAt,
                LastUpdatedBy = transaction.LastUpdatedBy
            };
        }
    }
}
