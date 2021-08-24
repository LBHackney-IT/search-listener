using AutoFixture;
using FluentAssertions;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using System.Linq;
using Xunit;

namespace HousingSearchListener.Tests.V1.Factories
{
    public class ESEntityFactoryTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly ESEntityFactory _sut = new ESEntityFactory();

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void CreatePersonTest(bool hasIds, bool hasTenures)
        {
            var ids = hasIds ? _fixture.CreateMany<Identification>(5).ToList() : null;
            var tenures = hasTenures ? _fixture.CreateMany<Tenure>(5).ToList() : null;
            var domainPerson = _fixture.Build<Person>()
                    .With(x => x.Identifications, ids)
                    .With(x => x.Tenures, tenures)
                    .Create();

            var result = _sut.CreatePerson(domainPerson);

            result.DateOfBirth.Should().Be(domainPerson.DateOfBirth);
            result.Firstname.Should().Be(domainPerson.FirstName);
            result.Id.Should().Be(domainPerson.Id);
            if (hasIds)
                result.Identifications.Should().BeEquivalentTo(domainPerson.Identifications);
            else
                result.Identifications.Should().BeEmpty();
            //result.IsPersonCautionaryAlerted.Should().Be();
            //result.IsTenureCautionaryAlerted.Should().Be();            
            result.MiddleName.Should().Be(domainPerson.MiddleName);
            result.PersonTypes.Should().BeEquivalentTo(domainPerson.PersonType);
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
            result.HouseholdMembers.Should().BeEquivalentTo(domainTenure.HouseholdMembers);
            result.Id.Should().Be(domainTenure.Id);
            result.PaymentReference.Should().Be(domainTenure.PaymentReference);
            result.StartOfTenureDate.Should().Be(domainTenure.StartOfTenureDate);
            result.TenuredAsset.Should().BeEquivalentTo(domainTenure.TenuredAsset);
            result.TenureType.Should().BeEquivalentTo(domainTenure.TenureType);
        }
    }
}
