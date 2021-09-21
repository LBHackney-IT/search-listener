using FluentAssertions;
using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Gateway;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.Gateway.Steps
{
    public class EsGatewaySteps : BaseSteps
    {
        private readonly ElasticSearchFixture _elasticSearchFixture;
        private readonly EsGateway _esGateway;
        private Exception _lastException;

        public EsGatewaySteps()
        {
            _elasticSearchFixture = new ElasticSearchFixture();
            _esGateway = new EsGateway(_elasticSearchFixture.ElasticSearchClient);
        }

        public async Task WhenIndexPersonIsTriggered(ESPerson esPerson)
        {
            async Task<IndexResponse> func()
            {
                return await _esGateway.IndexPerson(esPerson).ConfigureAwait(false);
            }

            _lastException = await Record.ExceptionAsync(func);
        }

        public async Task WhenPersonAlreadyExists(ESPerson esPerson)
        {
            await _esGateway.IndexPerson(esPerson).ConfigureAwait(false);

            var result = await _elasticSearchFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(esPerson.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            result.Should().NotBeNull();
            result.Source.Should().BeEquivalentTo(esPerson);

            _cleanup.Add(async () => await _elasticSearchFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("persons", esPerson.Id))
                                                                                    .ConfigureAwait(false));
        }

        public async Task WhenIndexTenureIsTriggered(QueryableTenure tenure)
        {
            async Task<IndexResponse> func()
            {
                return await _esGateway.IndexTenure(tenure).ConfigureAwait(false);
            }

            _lastException = await Record.ExceptionAsync(func);
        }

        public async Task WhenAddTenureToPersonIsTriggered(ESPerson esPerson, ESPersonTenure tenure)
        {
            async Task<UpdateResponse<Person>> func()
            {
                return await _esGateway.AddTenureToPersonAsync(esPerson, tenure).ConfigureAwait(false);
            }

            _lastException = await Record.ExceptionAsync(func);
        }

        public void ThenArgumentNullExceptionIsThrown()
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType<ArgumentNullException>();
        }

        public async Task ThenAPersonCreated(ESPerson person)
        {
            var result = await _elasticSearchFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            result.Should().NotBeNull();
            result.Source.Should().BeEquivalentTo(person);

            _cleanup.Add(async () => await _elasticSearchFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("persons", person.Id))
                                                                                    .ConfigureAwait(false));
        }

        public async Task ThenATenureCreated(QueryableTenure tenure)
        {
            var result = await _elasticSearchFixture.ElasticSearchClient
                                           .GetAsync<QueryableTenure>(tenure.Id, g => g.Index("tenures"))
                                           .ConfigureAwait(false);

            result.Should().NotBeNull();
            result.Source.Should().BeEquivalentTo(tenure);

            _cleanup.Add(async () => await _elasticSearchFixture.ElasticSearchClient.DeleteAsync(new DeleteRequest("tenures", tenure.Id))
                                                                                    .ConfigureAwait(false));
        }

        public async Task ThenAPersonAccountUpdated(ESPerson person, ESPersonTenure tenure)
        {
            var result = await _elasticSearchFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            var personResult = result.Source;
            personResult.Should().BeEquivalentTo(person, options => options.Excluding(_ => _.Tenures));

            var updatedTenure = personResult.Tenures.Where(x => x.Id.Equals(tenure.Id)).FirstOrDefault();
            updatedTenure.Should().NotBeNull();
            updatedTenure.TotalBalance.Should().Be(tenure.TotalBalance);
        }

        public async Task ThenAPersonAccountAdded(ESPerson person, ESPersonTenure tenure)
        {
            var result = await _elasticSearchFixture.ElasticSearchClient
                                           .GetAsync<ESPerson>(person.Id, g => g.Index("persons"))
                                           .ConfigureAwait(false);

            var personResult = result.Source;

            personResult.Should().BeEquivalentTo(person, options => options.Excluding(_ => _.Tenures));

            personResult.Tenures.Should().NotBeEmpty();
            personResult.Tenures.FindLast(_ => _.Id.Equals(tenure.Id)).Should().BeEquivalentTo(tenure);
        }
    }
}
