using HousingSearchListener.Tests.V1.E2ETests.Fixtures;
using HousingSearchListener.Tests.V1.E2ETests.Steps;
using System;
using TestStack.BDDfy;
using Xunit;

namespace HousingSearchListener.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "SQS Tenure Listener",
        IWant = "a function to process the account created message",
        SoThat = "The account details are set in the appropriate places in the asset, tenure and person indexes")]
    [Collection("ElasticSearch collection")]
    public class UpdateAccountDetailsTests : IDisposable
    {
        private readonly ElasticSearchFixture _esFixture;
        private readonly AccountApiFixture _accountApiFixture;
        private readonly TenureApiFixture _tenureApiFixture;

        private readonly UpdateAccountDetailsSteps _steps;

        public UpdateAccountDetailsTests(ElasticSearchFixture esFixture)
        {
            _esFixture = esFixture;
            _accountApiFixture = new AccountApiFixture();
            _tenureApiFixture = new TenureApiFixture();

            _steps = new UpdateAccountDetailsSteps();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _accountApiFixture.Dispose();
                _tenureApiFixture.Dispose();

                _disposed = true;
            }
        }

        [Fact]
        public void AccountNotFound()
        {
            var accountId = Guid.NewGuid();
            this.Given(g => _accountApiFixture.GivenTheAccountDoesNotExist(accountId))
                .When(w => _steps.WhenTheFunctionIsTriggered(accountId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_accountApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenAnAccountNotFoundExceptionIsThrown(accountId))
                .BDDfy();
        }

        [Fact]
        public void TenureNotFound()
        {
            var accountId = Guid.NewGuid();
            this.Given(g => _accountApiFixture.GivenTheAccountExists(accountId))
                .And(h => _tenureApiFixture.GivenTheTenureDoesNotExist(_accountApiFixture.ResponseObject.TargetId))
                .When(w => _steps.WhenTheFunctionIsTriggered(accountId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_accountApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenATenureNotFoundExceptionIsThrown(_accountApiFixture.ResponseObject.TargetId))
                .BDDfy();
        }

        [Fact]
        public void AllIndexesUpdated()
        {
            var accountId = Guid.NewGuid();
            this.Given(g => _accountApiFixture.GivenTheAccountExists(accountId))
                .And(h => _tenureApiFixture.GivenTheTenureExists(_accountApiFixture.ResponseObject.TargetId))
                .And(i => _esFixture.GivenATenureIsIndexed(_tenureApiFixture.ResponseObject))
                .And(i => _esFixture.GivenAnAssetIsIndexed(_tenureApiFixture.ResponseObject.TenuredAsset.Id, _tenureApiFixture.ResponseObject.Id))
                .And(i => _esFixture.GivenTenurePersonsAreIndexed(_tenureApiFixture.ResponseObject))
                .When(w => _steps.WhenTheFunctionIsTriggered(accountId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_accountApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenTheAssetIndexIsUpdated(_accountApiFixture.ResponseObject.PaymentReference,
                                                             _tenureApiFixture.ResponseObject.TenuredAsset.Id, _esFixture.ElasticSearchClient))
                .Then(t => _steps.ThenTheTenureIndexIsUpdated(_accountApiFixture.ResponseObject.PaymentReference,
                                                              _tenureApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .Then(t => _steps.ThenThePersonIndexIsUpdated(_accountApiFixture.ResponseObject.PaymentReference,
                                                              _tenureApiFixture.ResponseObject, _esFixture.ElasticSearchClient))
                .BDDfy();
        }
    }
}
