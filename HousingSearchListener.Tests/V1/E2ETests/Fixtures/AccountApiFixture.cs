using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;
using HousingSearchListener.V1.Boundary.Response;
using System;

namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class AccountApiFixture : BaseApiFixture<AccountResponse>
    {
        private readonly Fixture _fixture = new Fixture();

        public AccountApiFixture()
            : base(FixtureConstants.AccountApiRoute, FixtureConstants.AccountApiToken)
        {
            Environment.SetEnvironmentVariable("AccountApiUrl", FixtureConstants.AccountApiRoute);
            Environment.SetEnvironmentVariable("AccountApiToken", FixtureConstants.AccountApiToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }

        private AccountResponse ConstructAccountResponseObject(Guid id)
        {
            return _fixture.Build<AccountResponse>()
                                                 .With(x => x.Id, id)
                                                 .With(x => x.StartDate, DateTime.UtcNow.AddMonths(-1))
                                                 .With(x => x.EndDate, (DateTime?)null)
                                                 .With(x => x.CreatedAt, DateTime.UtcNow.AddMinutes(-60))
                                                 .With(x => x.LastUpdatedAt, DateTime.UtcNow.AddMinutes(-60))
                                                 .Create();
        }

        public void GivenTheAccountDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public AccountResponse GivenTheAccountExists(Guid id)
        {
            ResponseObject = ConstructAccountResponseObject(id);

            return ResponseObject;
        }
    }
}
