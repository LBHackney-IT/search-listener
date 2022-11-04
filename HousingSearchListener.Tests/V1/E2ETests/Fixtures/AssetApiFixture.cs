using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;
using Hackney.Shared.HousingSearch.Domain.Asset;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using System;

namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class AssetApiFixture : BaseApiFixture<QueryableAsset>
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

        public QueryableAsset GivenTheAssetExists(Guid id)
        {
            ResponseObject = _fixture.Build<QueryableAsset>()
                                     .With(x => x.Id, id.ToString())
                                     .With(x => x.AssetContract, _fixture.Build<QueryableAssetContract>()
                                         .With(c => c.TargetId, id.ToString())
                                         .With(c => c.TargetType, "asset")
                                         .Create())
                                     .Create();

            return ResponseObject;
        }
    }
}
