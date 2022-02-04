﻿using AutoFixture;
using FluentAssertions;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using System;
using System.Collections.Generic;
using System.Linq;
using HousingSearchListener.V1.Domain.Transaction;
using Xunit;
using Person = HousingSearchListener.V1.Domain.Person.Person;
using HousingSearchListener.V1.Factories.QueryableFactories;

namespace HousingSearchListener.Tests.V1.Factories
{
    public class ESEntityFactoryTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly PersonFactory _personFactory = new PersonFactory();
        private readonly TenuresFactory _tenuresFactory = new TenuresFactory();
        private readonly TransactionsFactory _transactionsFactory = new TransactionsFactory();

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreatePersonTest(bool hasTenures)
        {
            var tenures = hasTenures ? _fixture.CreateMany<Tenure>(5).ToList() : null;
            var domainPerson = _fixture.Build<Person>()
                    .With(x => x.Tenures, tenures)
                    .Create();

            var result = _personFactory.CreatePerson(domainPerson);

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

            var result = _tenuresFactory.CreateQueryableTenure(domainTenure);
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
            var result = _tenuresFactory.CreateQueryableHouseholdMembers(null);
            result.Should().BeEmpty();
        }

        [Fact]
        public void CreateQueryableHouseholdMembersTestHasInput()
        {
            var domainHms = _fixture.CreateMany<HouseholdMembers>(5).ToList();

            var result = _tenuresFactory.CreateQueryableHouseholdMembers(domainHms);
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
            Action act = () => _tenuresFactory.CreateAssetQueryableTenure(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateAssetQueryableTenureTest()
        {
            var domainTenure = _fixture.Create<TenureInformation>();

            var result = _tenuresFactory.CreateAssetQueryableTenure(domainTenure);
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

            var result = _transactionsFactory.CreateQueryableTransaction(domainTransaction);
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
    }
}
