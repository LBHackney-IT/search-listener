using AutoFixture;
using FluentAssertions;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using HousingSearchListener.V1.Domain.Transaction;
using Xunit;
using Person = HousingSearchListener.V1.Domain.Person.Person;
using Tenure = HousingSearchListener.V1.Domain.Person.Tenure;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Domain.Process;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;

namespace HousingSearchListener.Tests.V1.Factories
{
    public class ESEntityFactoryTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly ESEntityFactory _sut = new ESEntityFactory();

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreatePersonTest(bool hasTenures)
        {
            var tenures = hasTenures ? _fixture.CreateMany<Tenure>(5).ToList() : null;
            var domainPerson = _fixture.Build<Person>()
                    .With(x => x.Tenures, tenures)
                    .Create();

            var result = _sut.CreatePerson(domainPerson);

            result.DateOfBirth.Should().Be(domainPerson.DateOfBirth);
            result.Firstname.Should().Be(domainPerson.FirstName);
            result.Id.Should().Be(domainPerson.Id);
            result.Middlename.Should().Be(domainPerson.MiddleName);
            result.PersonTypes.Should().BeEquivalentTo(domainPerson.PersonTypes);
            result.PreferredFirstname.Should().Be(domainPerson.PreferredFirstName);
            result.PreferredSurname.Should().Be(domainPerson.PreferredSurname);
            result.Surname.Should().Be(domainPerson.Surname);
            if (hasTenures)
                result.Tenures.Should().BeEquivalentTo(domainPerson.Tenures,
                                                       y => y.Excluding(z => z.AssetId)
                                                             .Excluding(z => z.Uprn)
                                                             .Excluding(z => z.IsActive));
            else
                result.Tenures.Should().BeEmpty();
            result.Title.Should().Be(domainPerson.Title);
        }

        [Fact]
        public void CreateQueryableTenureTest()
        {
            var domainTenure = _fixture.Create<TenureInformation>();

            var result = _sut.CreateQueryableTenure(domainTenure);
            result.EndOfTenureDate.Should().Be(domainTenure.EndOfTenureDate);
            VerifyHouseholdMembers(result.HouseholdMembers, domainTenure.HouseholdMembers);
            result.Id.Should().Be(domainTenure.Id);
            result.PaymentReference.Should().Be(domainTenure.PaymentReference);
            result.StartOfTenureDate.Should().Be(domainTenure.StartOfTenureDate);
            result.TenuredAsset.Should().BeEquivalentTo(domainTenure.TenuredAsset);
            result.TenureType.Should().BeEquivalentTo(domainTenure.TenureType);
        }

        [Fact]
        public void CreateQueryableHouseholdMembersTestNoInput()
        {
            var result = _sut.CreateQueryableHouseholdMembers(null);
            result.Should().BeEmpty();
        }

        [Fact]
        public void CreateQueryableHouseholdMembersTestHasInput()
        {
            var domainHms = _fixture.CreateMany<HouseholdMembers>(5).ToList();

            var result = _sut.CreateQueryableHouseholdMembers(domainHms);
            VerifyHouseholdMembers(result, domainHms);
        }

        private static void VerifyHouseholdMembers(List<QueryableHouseholdMember> actual, List<HouseholdMembers> expected)
        {
            actual.Count.Should().Be(expected.Count);
            actual.Select(x => x.Id).Should().BeEquivalentTo(expected.Select(y => y.Id));
            foreach (var esHm in actual)
            {
                var domainHm = expected.First(x => x.Id == esHm.Id);
                esHm.DateOfBirth.Should().Be(domainHm.DateOfBirth);
                esHm.FullName.Should().Be(domainHm.FullName);
                esHm.Id.Should().Be(domainHm.Id);
                esHm.IsResponsible.Should().Be(domainHm.IsResponsible);
                esHm.PersonTenureType.Should().Be(domainHm.PersonTenureType);
                esHm.Type.Should().Be(domainHm.Type);
            }
        }

        [Fact]
        public void CreateAssetQueryableTenureTestNullInputThrows()
        {
            Action act = () => _sut.CreateAssetQueryableTenure(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateAssetQueryableTenureTest()
        {
            var domainTenure = _fixture.Create<TenureInformation>();

            var result = _sut.CreateAssetQueryableTenure(domainTenure);
            result.EndOfTenureDate.Should().Be(domainTenure.EndOfTenureDate);
            result.Id.Should().Be(domainTenure.Id);
            result.PaymentReference.Should().Be(domainTenure.PaymentReference);
            result.StartOfTenureDate.Should().Be(domainTenure.StartOfTenureDate);
            result.Type.Should().Be(domainTenure.TenureType.Description);
        }
        [Fact]
        public void CreateQueryableTransactionTest()
        {
            var domainTransaction = _fixture.Create<TransactionResponseObject>();

            var result = _sut.CreateQueryableTransaction(domainTransaction);
            result.Address.Should().Be(domainTransaction.Address);
            result.Id.Should().Be(domainTransaction.Id);
            result.PaymentReference.Should().Be(domainTransaction.PaymentReference);
            result.BalanceAmount.Should().Be(domainTransaction.BalanceAmount);
            result.BankAccountNumber.Should().Be(domainTransaction.BankAccountNumber);
            result.ChargedAmount.Should().Be(domainTransaction.ChargedAmount);
            result.FinancialMonth.Should().Be(domainTransaction.FinancialMonth);
            result.FinancialYear.Should().Be(domainTransaction.FinancialYear);
            result.Fund.Should().Be(domainTransaction.Fund);
            result.HousingBenefitAmount.Should().Be(domainTransaction.HousingBenefitAmount);
            result.PaidAmount.Should().Be(domainTransaction.PaidAmount);
            result.PeriodNo.Should().Be(domainTransaction.PeriodNo);
            result.TargetId.Should().Be(domainTransaction.TargetId);
            result.TargetType.Should().Be(domainTransaction.TargetType);
            result.TransactionAmount.Should().Be(domainTransaction.TransactionAmount);
            result.TransactionDate.Should().Be(domainTransaction.TransactionDate);
            result.TransactionSource.Should().Be(domainTransaction.TransactionSource);
            result.TransactionType.Should().Be(domainTransaction.TransactionType);
            result.SortCode.Should().Be(domainTransaction.SortCode);
            result.SuspenseResolutionInfo.IsApproved.Should().Be(domainTransaction.SuspenseResolutionInfo.IsApproved);
            result.SuspenseResolutionInfo.IsConfirmed.Should().Be(domainTransaction.SuspenseResolutionInfo.IsConfirmed);
            result.SuspenseResolutionInfo.Note.Should().Be(domainTransaction.SuspenseResolutionInfo.Note);
            result.SuspenseResolutionInfo.ResolutionDate.Should().Be(domainTransaction.SuspenseResolutionInfo.ResolutionDate);

            result.Sender.FullName.Should().Be(domainTransaction.Person.FullName);
            result.Sender.Id.Should().Be(domainTransaction.Person.Id);
        }

        [Fact]
        public void CreateAssetTest()
        {

            var domainAsset = _fixture.Build<Asset>()
            .With(x => x.AssetAddress, _fixture.Create<AssetAddress>())
            .With(x => x.AssetCharacteristics, _fixture.Create<AssetCharacteristics>())
            .With(x => x.AssetManagement, _fixture.Create<AssetManagement>())
            .With(x => x.AssetLocation, _fixture.Create<AssetLocation>())
            .Create();

            var result = _sut.CreateAsset(domainAsset);
            result.Id.Should().Be(domainAsset.Id.ToString());
            result.AssetId.Should().Be(domainAsset.AssetId);
            result.AssetAddress.AddressLine1.Should().Be(domainAsset.AssetAddress.AddressLine1);
            result.AssetAddress.PostCode.Should().Be(domainAsset.AssetAddress.PostCode);
            result.AssetType.Should().Be(domainAsset.AssetType.ToString());
            result.AssetCharacteristics.IsStepFree.Should().Be(domainAsset.AssetCharacteristics.IsStepFree);
            result.AssetCharacteristics.NumberOfBedrooms.Should().Be(domainAsset.AssetCharacteristics.NumberOfBedrooms);
            result.AssetCharacteristics.HasPrivateBathroom.Should().Be(domainAsset.AssetCharacteristics.HasPrivateBathroom);
            result.AssetManagement.PropertyOccupiedStatus.Should().Be(domainAsset.AssetManagement.PropertyOccupiedStatus);
            result.AssetLocation.FloorNo.Should().Be(domainAsset.AssetLocation.FloorNo);
            result.ParentAssetIds.Should().Be(domainAsset.ParentAssetIds);
            result.RootAsset.Should().Be(domainAsset.RootAsset);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreatesQueryableProcessFromProcessRecord(bool hasPreviousStates)
        {
            // Given
            var process = _fixture.Create<Process>();
            if (!hasPreviousStates) process.PreviousStates = new List<ProcessState>();

            // When
            var result = _sut.CreateProcess(process);

            // Then
            result.Id.Should().Be(process.Id.ToString());
            result.TargetId.Should().Be(process.TargetId.ToString());
            result.TargetType.Should().Be(process.TargetType.ToString());
            result.ProcessName.Should().Be(process.ProcessName.ToString());
            result.State.Should().Be(process.CurrentState.State);

            var createdAt = process.PreviousStates.Count > 0 ? process.PreviousStates.Min(x => x.CreatedAt) : process.CurrentState.CreatedAt;
            result.CreatedAt.Should().Be(createdAt.ToLongDateString());

            foreach (var relatedEntity in result.RelatedEntities)
            {
                var processRelatedEntity = process.RelatedEntities.Find(x => x.Id.ToString() == relatedEntity.Id);
                relatedEntity.TargetType.Should().BeEquivalentTo(processRelatedEntity.TargetType);
                relatedEntity.SubType.Should().BeEquivalentTo(processRelatedEntity.SubType);
                relatedEntity.Description.Should().BeEquivalentTo(processRelatedEntity.Description);
            }
        }
    }
}
