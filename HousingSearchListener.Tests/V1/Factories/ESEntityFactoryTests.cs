using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

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
            //result.IsPersonCautionaryAlerted.Should().Be();
            //result.IsTenureCautionaryAlerted.Should().Be();            
            result.MiddleName.Should().Be(domainPerson.MiddleName);
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
            result.TenuredAsset.Should().BeEquivalentTo(domainTenure.TenuredAsset);
            result.Type.Should().Be(domainTenure.TenureType.Description);
        }
    }
}
