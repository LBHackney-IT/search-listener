using AutoFixture;
using HousingSearchListener.V1.Domain.Person;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class PersonApiFixture : BaseApiFixture<Person>
    {
        public PersonApiFixture()
        {
            Environment.SetEnvironmentVariable("PersonApiUrl", FixtureConstants.PersonApiRoute);
            Environment.SetEnvironmentVariable("PersonApiToken", FixtureConstants.PersonApiToken);

            _route = FixtureConstants.PersonApiRoute;
            _token = FixtureConstants.PersonApiToken;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                ResponseObject = null;
                base.Dispose(disposing);
            }
        }

        public void GivenThePersonDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public Person GivenThePersonExists(Guid id)
        {
            ResponseObject = _fixture.Build<Person>()
                                     .With(x => x.Id, id.ToString())
                                     .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                                     .With(x => x.Tenures, _fixture.CreateMany<Tenure>(1).ToList())
                                     .Create();

            ResponseObject.Tenures.First().Id = Guid.NewGuid().ToString();

            return ResponseObject;
        }

        public Person GivenThePersonExistsWithTenure(Guid id, Guid tenureId)
        {
            var personTypes = new List<string> { "Tenant", "HouseholderMember", "Freeholder" };
            ResponseObject = _fixture.Build<Person>()
                                     .With(x => x.Id, id.ToString())
                                     .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                                     .With(x => x.PersonTypes, personTypes)
                                     .With(x => x.Tenures, _fixture.CreateMany<Tenure>(3).ToList())
                                     .Create();

            ResponseObject.Tenures.Last().Id = tenureId.ToString();

            return ResponseObject;
        }
    }
}
