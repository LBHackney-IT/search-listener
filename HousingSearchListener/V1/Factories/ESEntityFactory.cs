using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Domain.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using Person = HousingSearchListener.V1.Domain.Person.Person;
using QueryableTenuredAsset = Hackney.Shared.HousingSearch.Gateways.Models.Tenures.QueryableTenuredAsset;

namespace HousingSearchListener.V1.Factories
{
    public class ESEntityFactory : IESEntityFactory
    {
        private List<QueryablePersonTenure> CreatePersonTenures(List<Tenure> tenures)
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

        public QueryableTenure CreateQueryableTenure(TenureInformation tenure)
        {
            return new QueryableTenure
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


        public QueryableAsset CreateAsset(Asset asset)
        {
            QueryableAsset queryableAsset = new QueryableAsset();
            QueryableAssetAddress assetAddress = new QueryableAssetAddress();
            QueryableAssetTenure assetTenure = new QueryableAssetTenure();
            QueryableAssetCharacteristics assetCharacteristics = new QueryableAssetCharacteristics();
            QueryableAssetManagement assetManagement = new QueryableAssetManagement();

            queryableAsset.Id = asset.Id.ToString();
            queryableAsset.AssetId = asset.AssetId;
            queryableAsset.AssetType = asset.AssetType.ToString();
            queryableAsset.ParentAssetIds = asset.ParentAssetIds;
            queryableAsset.RootAsset = asset.RootAsset;

            assetAddress.AddressLine1 = asset.AssetAddress.AddressLine1;
            assetAddress.AddressLine2 = asset.AssetAddress.AddressLine2;
            assetAddress.AddressLine3 = asset.AssetAddress.AddressLine3;
            assetAddress.AddressLine4 = asset.AssetAddress.AddressLine4;
            assetAddress.PostCode = asset.AssetAddress.PostCode;
            assetAddress.Uprn = asset.AssetAddress.Uprn;
            assetAddress.PostPreamble = asset.AssetAddress.PostPreamble;
            queryableAsset.AssetAddress = assetAddress;

            assetTenure.Id = asset.Tenure.Id;
            assetTenure.StartOfTenureDate = asset.Tenure.StartOfTenureDate.ToString();
            assetTenure.EndOfTenureDate = asset.Tenure.EndOfTenureDate.ToString();
            assetTenure.PaymentReference = asset.Tenure.PaymentReference;
            assetTenure.Type = asset.Tenure.Type;
            queryableAsset.Tenure = assetTenure;

            assetCharacteristics.HasStairs = asset.AssetCharacteristics.HasStairs;
            assetCharacteristics.NumberOfBedrooms = asset.AssetCharacteristics.NumberOfBedrooms;
            assetCharacteristics.NumberOfBedSpaces = asset.AssetCharacteristics.NumberOfBedSpaces;
            assetCharacteristics.NumberOfCots = asset.AssetCharacteristics.NumberOfCots;
            assetCharacteristics.NumberOfLifts = asset.AssetCharacteristics.NumberOfLifts;
            assetCharacteristics.NumberOfLivingRooms = asset.AssetCharacteristics.NumberOfLivingRooms;
            assetCharacteristics.HasPrivateBathroom = asset.AssetCharacteristics.HasPrivateBathroom;
            assetCharacteristics.HasPrivateKitchen = asset.AssetCharacteristics.HasPrivateKitchen;
            assetCharacteristics.IsStepFree = asset.AssetCharacteristics.IsStepFree;
            assetCharacteristics.WindowType = asset.AssetCharacteristics.WindowType;
            assetCharacteristics.YearConstructed = asset.AssetCharacteristics.YearConstructed;
            queryableAsset.AssetCharacteristics = assetCharacteristics;

            assetManagement.Agent = asset.AssetManagement.Agent;
            assetManagement.AreaOfficeName = asset.AssetManagement.AreaOfficeName;
            assetManagement.IsCouncilProperty = asset.AssetManagement.IsCouncilProperty;
            assetManagement.IsNoRepairsMaintenance = asset.AssetManagement.IsNoRepairsMaintenance;
            assetManagement.IsTMOManaged = asset.AssetManagement.IsTMOManaged;
            assetManagement.ManagingOrganisation = asset.AssetManagement.ManagingOrganisation;
            assetManagement.ManagingOrganisationId = asset.AssetManagement.ManagingOrganisationId;
            assetManagement.Owner = asset.AssetManagement.Owner;
            assetManagement.PropertyOccupiedStatus = asset.AssetManagement.PropertyOccupiedStatus;
            queryableAsset.AssetManagement = assetManagement;

            return queryableAsset;
        }
    }
}