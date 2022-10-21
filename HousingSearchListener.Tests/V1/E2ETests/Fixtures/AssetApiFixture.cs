using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;
using Hackney.Shared.Asset.Domain;
using System;

namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class AssetApiFixture : BaseApiFixture<Hackney.Shared.HousingSearch.Domain.Asset.Asset>
    {
        private readonly Fixture _fixture = new Fixture();

        public AssetApiFixture()
            : base(FixtureConstants.AssetApiRoute, FixtureConstants.AssetApiToken)
        {
            Environment.SetEnvironmentVariable("AssetApiUrl", FixtureConstants.AssetApiRoute);
            Environment.SetEnvironmentVariable("AssetApiToken", FixtureConstants.AssetApiToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }

        public void GivenTheAssetDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public Hackney.Shared.HousingSearch.Domain.Asset.Asset GivenTheAssetExists(Guid id)
        {
            ResponseObject = _fixture.Build<Hackney.Shared.HousingSearch.Domain.Asset.Asset>()
                                     .With(x => x.Id, id.ToString())
                                     .Create();

            return ResponseObject;
        }
    }
}
