using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;
using HousingSearchListener.V1.Domain.Account;
using System;

namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class AccountApiFixture : BaseApiFixture<AccountResponseObject>
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

        private AccountResponseObject ConstructAccountResponseObject(Guid id)
        {
            return _fixture.Build<AccountResponseObject>()
                                                 .With(x => x.Id, id)
                                                 .With(x => x.StartDate, DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                                                 .With(x => x.EndDate, "")
                                                 .With(x => x.CreatedAt, DateTime.UtcNow.AddMinutes(-60).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                                                 .With(x => x.LastUpdatedAt, DateTime.UtcNow.AddMinutes(-60).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                                                 .Create();
        }

        public void GivenTheAccountDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public AccountResponseObject GivenTheAccountExists(Guid id)
        {
            ResponseObject = ConstructAccountResponseObject(id);

            return ResponseObject;
        }
    }
}
