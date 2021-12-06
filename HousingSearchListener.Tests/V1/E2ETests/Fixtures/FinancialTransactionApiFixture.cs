using System;
using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;
using HousingSearchListener.V1.Domain.Transaction;

namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class FinancialTransactionApiFixture : BaseApiFixture<TransactionResponseObject>
    {
        private readonly Fixture _fixture = new Fixture();
        public FinancialTransactionApiFixture()
            : base(FixtureConstants.FinancialTransactionApiRoute, FixtureConstants.FinancialTransactionApiToken)
        {
            Environment.SetEnvironmentVariable("FinancialTransactionApiUrl", FixtureConstants.FinancialTransactionApiRoute);
            Environment.SetEnvironmentVariable("FinancialTransactionApiToken", FixtureConstants.FinancialTransactionApiToken);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }
        private TransactionResponseObject ConstructTransactionResponseObject(Guid id, Guid targetId)
        {
            return _fixture.Build<TransactionResponseObject>()
                .With(x => x.Id, id)
                .With(x => x.TargetId, targetId)
                .Create();
        }

        public void GivenTheTransactionDoesNotExist(Guid id, Guid targetId)
        {
            // Nothing to do here
        }

        public TransactionResponseObject GivenTheTransactionExists(Guid id, Guid targetId)
        {
            ResponseObject = ConstructTransactionResponseObject(id, targetId);

            return ResponseObject;
        }
    }
}
