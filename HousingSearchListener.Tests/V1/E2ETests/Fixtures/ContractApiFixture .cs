using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;
using Hackney.Shared.HousingSearch.Domain.Contract;
using System;
using System.Collections.Generic;

namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class ContractApiFixture : BaseApiFixture<Contract>
    {
        private readonly Fixture _fixture = new Fixture();

        public ContractApiFixture()
            : base(FixtureConstants.ContractsApiRoute, FixtureConstants.ContractsApiToken)
        {
            Environment.SetEnvironmentVariable("ContractApiUrl", FixtureConstants.ContractsApiRoute);
            Environment.SetEnvironmentVariable("ContractApiToken", FixtureConstants.ContractsApiToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }

        public void GivenTheContractDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public Contract GivenTheContractExists(Guid contractId, Guid targetId)
        {
            ResponseObject = _fixture.Build<Contract>()
                                     .With(x => x.Id, contractId.ToString())
                                     .With(x => x.TargetId, targetId.ToString())
                                     .With(x => x.TargetType, "asset")
                                     .Create();

            return ResponseObject;
        }
    }
}
