using FluentAssertions;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Boundary.Response;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories.QueryableFactories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class UpdateAccountDetailsSteps : BaseSteps
    {
        private readonly TenuresFactory _tenureFactory = new TenuresFactory();

        public UpdateAccountDetailsSteps()
        {
            _eventType = EventTypes.AccountUpdatedEvent;
        }

        public async Task WhenTheFunctionIsTriggered(Guid accountId)
        {
            await TriggerFunction(accountId);
        }

        public void TheNoExceptionIsThrown()
        {
            _lastException.Should().BeNull();
        }

        public void ThenAnAccountNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<AccountResponse>));
            (_lastException as EntityNotFoundException<AccountResponse>).Id.Should().Be(id);
        }

        public void ThenATenureNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<TenureInformation>));
            (_lastException as EntityNotFoundException<TenureInformation>).Id.Should().Be(id);
        }

        public async Task ThenTheAssetIndexIsUpdated(string newPaymentReference, string assetId, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableAsset>(assetId, g => g.Index("assets"))
                                       .ConfigureAwait(false);

            var assetInIndex = result.Source;
            assetInIndex.Tenure.PaymentReference.Should().Be(newPaymentReference);
        }

        public async Task ThenTheTenureIndexIsUpdated(
            string newPaymentReference, TenureInformation tenure, IElasticClient esClient)
        {
            var result = await esClient.GetAsync<QueryableTenure>(tenure.Id, g => g.Index("tenures"))
                                       .ConfigureAwait(false);

            var tenureInIndex = result.Source;
            tenureInIndex.Should().BeEquivalentTo(_tenureFactory.CreateQueryableTenure(tenure),
                                                  c => c.Excluding(y => y.PaymentReference));
            tenureInIndex.PaymentReference.Should().Be(newPaymentReference);
        }

        public async Task ThenThePersonIndexIsUpdated(
            string newPaymentReference,
            decimal newTotalBalance,
            TenureInformation tenure,
            IElasticClient esClient)
        {
            foreach (var hm in tenure.HouseholdMembers)
            {
                var result = await esClient.GetAsync<QueryablePerson>(hm.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);
                var personInIndex = result.Source;

                var personTenure = personInIndex.Tenures.First(x => x.Id == tenure.Id);
                personTenure.PaymentReference.Should().Be(newPaymentReference);
                personTenure.TotalBalance.Should().Be(newTotalBalance);
            }
        }
    }
}
