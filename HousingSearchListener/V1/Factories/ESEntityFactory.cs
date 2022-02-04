using Hackney.Shared.HousingSearch.Domain.Accounts;
using Hackney.Shared.HousingSearch.Gateways.Models.Accounts;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Domain.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using QueryableAccountTenure = Hackney.Shared.HousingSearch.Gateways.Models.Accounts.QueryableTenure;
using QueryableTenure = Hackney.Shared.HousingSearch.Gateways.Models.Tenures.QueryableTenure;
using Person = HousingSearchListener.V1.Domain.Person.Person;
using QueryableTenuredAsset = Hackney.Shared.HousingSearch.Gateways.Models.Tenures.QueryableTenuredAsset;

namespace HousingSearchListener.V1.Factories
{
    public class ESEntityFactory : IESEntityFactory
    {
        private List<QueryablePersonTenure> CreatePersonTenures(List<V1.Domain.Person.Tenure> tenures)
        {
            return tenures.Select(x => new QueryablePersonTenure
            {
                AssetFullAddress = x.AssetFullAddress,
                EndDate = x.EndDate,
                Id = x.Id,
                StartDate = x.StartDate,
                Type = x.Type
            }).ToList();
        }

        public QueryablePerson CreatePerson(Person person)
        {
            return new QueryablePerson
            {
                Id = person.Id,
                DateOfBirth = person.DateOfBirth,
                Title = person.Title,
                Firstname = person.FirstName,
                Surname = person.Surname,
                Middlename = person.MiddleName,
                PreferredFirstname = person.PreferredFirstName,
                PreferredSurname = person.PreferredSurname,
                PersonTypes = person.PersonTypes,
                Tenures = person.Tenures != null ? CreatePersonTenures(person.Tenures) : new List<QueryablePersonTenure>()
            };
        }

        public Hackney.Shared.HousingSearch.Gateways.Models.Tenures.QueryableTenure CreateQueryableTenure(TenureInformation tenure)
        {
            return new Hackney.Shared.HousingSearch.Gateways.Models.Tenures.QueryableTenure
            {
                Id = tenure.Id,
                StartOfTenureDate = tenure.StartOfTenureDate,
                EndOfTenureDate = tenure.EndOfTenureDate,
                TenureType = new QueryableTenureType()
                {
                    Code = tenure.TenureType.Code,
                    Description = tenure.TenureType.Description
                },
                PaymentReference = tenure.PaymentReference,
                HouseholdMembers = CreateQueryableHouseholdMembers(tenure.HouseholdMembers),
                TenuredAsset = new QueryableTenuredAsset()
                {
                    FullAddress = tenure.TenuredAsset?.FullAddress,
                    Id = tenure.TenuredAsset?.Id,
                    Type = tenure.TenuredAsset?.Type,
                    Uprn = tenure.TenuredAsset?.Uprn
                }
            };
        }

        public List<QueryableHouseholdMember> CreateQueryableHouseholdMembers(List<HouseholdMembers> householdMembers)
        {
            if (householdMembers is null) return new List<QueryableHouseholdMember>();

            return householdMembers.Select(x => new QueryableHouseholdMember()
            {
                DateOfBirth = x.DateOfBirth,
                FullName = x.FullName,
                Id = x.Id,
                IsResponsible = x.IsResponsible,
                PersonTenureType = x.PersonTenureType,
                Type = x.Type
            }).ToList();
        }

        public QueryableAssetTenure CreateAssetQueryableTenure(TenureInformation tenure)
        {
            if (tenure is null) throw new ArgumentNullException(nameof(tenure));

            return new QueryableAssetTenure()
            {
                Id = tenure.Id,
                EndOfTenureDate = tenure.EndOfTenureDate,
                PaymentReference = tenure.PaymentReference,
                StartOfTenureDate = tenure.StartOfTenureDate,
                Type = tenure.TenureType.Description
            };
        }

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

        public QueryableAccount ToQueryableAccount(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (account.Tenure == null)
                throw new Exception("There is no tenure provided for this account.");

            return new QueryableAccount
            {
                Id = account.Id,
                PaymentReference = account.PaymentReference,
                AccountBalance = account.AccountBalance,
                AccountStatus = account.AccountStatus,
                AccountType = account.AccountType,
                AgreementType = account.AgreementType,
                ConsolidatedBalance = account.ConsolidatedBalance,
                CreatedAt = account.CreatedAt,
                CreatedBy = account.CreatedBy,
                EndDate = account.EndDate,
                LastUpdatedAt = account.LastUpdatedAt,
                LastUpdatedBy = account.LastUpdatedBy,
                ParentAccountId = account.ParentAccountId,
                RentGroupType = account.RentGroupType,
                StartDate = account.StartDate,
                TargetId = account.TargetId,
                TargetType = account.TargetType,
                ConsolidatedCharges = (List<QueryableConsolidatedCharge>)(account.ConsolidatedCharges?.Select(p =>
                    new QueryableConsolidatedCharge
                    {
                        Amount = p.Amount,
                        Frequency = p.Frequency,
                        Type = p.Type
                    })),
                Tenure = new QueryableAccountTenure
                {
                    FullAddress = account.Tenure.FullAddress,
                    PrimaryTenants = account.Tenure?.PrimaryTenants.Select(s =>
                        new QueryablePrimaryTenant
                        {
                            Id = s.Id,
                            FullName = s.FullName
                        }).ToList()
                }
            };
        }
    }
}