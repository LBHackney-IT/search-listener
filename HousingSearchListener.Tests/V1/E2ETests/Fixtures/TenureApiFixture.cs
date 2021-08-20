using AutoFixture;
using HousingSearchListener.V1.Domain.Tenure;
using System;

namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class TenureApiFixture : BaseApiFixture<TenureInformation>
    {
        public TenureApiFixture()
        {
            Environment.SetEnvironmentVariable("TenureApiUrl", FixtureConstants.TenureApiRoute);
            Environment.SetEnvironmentVariable("TenureApiToken", FixtureConstants.TenureApiToken);

            _route = FixtureConstants.TenureApiRoute;
            _token = FixtureConstants.TenureApiToken;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                ResponseObject = null;
                base.Dispose(disposing);
            }
        }

        public void GivenTheTenureDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public TenureInformation GivenTheTenureExists(Guid id)
        {
            ResponseObject = _fixture.Build<TenureInformation>()
                                     .With(x => x.Id, id.ToString())
                                     .Create();
            return ResponseObject;
        }
    }
}
