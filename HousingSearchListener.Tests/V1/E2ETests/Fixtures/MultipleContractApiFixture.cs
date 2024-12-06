using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;
using Hackney.Shared.HousingSearch.Domain.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class MultipleContractApiFixture : BaseApiFixture<IEnumerable<Contract>>
    {
        private readonly Fixture _fixture = new Fixture();

        public MultipleContractApiFixture()
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


        public List<Contract> GivenMultipleContractsExist(Guid contractId, Guid targetId)
        {
            var ResponseObject = _fixture.Build<Contract>()
                                     .With(x => x.Id, contractId.ToString())
                                     .With(x => x.TargetId, targetId.ToString())
                                     .With(x => x.TargetType, "asset")
                                     .Create();

            //return ResponseObject.ToList();     BEWARE OF THE LIES! THIS DOES NOT IN FACT MAKE A LIST -__-  
            return new List<Contract> { ResponseObject };


            /*var singleContract = new Contract    
            {
                Id = contractId.ToString(),
                TargetId = contractId.ToString(),
                TargetType = "asset"
            };

            return new List<Contract> {singleContract};*/

        }
    }
}
