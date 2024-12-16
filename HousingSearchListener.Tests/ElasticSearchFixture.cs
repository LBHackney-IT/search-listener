using AutoFixture;
using Elasticsearch.Net;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using Microsoft.Extensions.Hosting;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;
using HousingSearchListener.V1.Domain.Transaction;
using Xunit;
using Person = HousingSearchListener.V1.Domain.Person.Person;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;

namespace HousingSearchListener.Tests
{
    public class ElasticSearchFixture
    {
        private readonly Fixture _fixture = new Fixture();

        public IElasticClient ElasticSearchClient => _factory?.ElasticSearchClient;

        private readonly MockApplicationFactory _factory;
        private readonly IHost _host;
        private readonly ESEntityFactory _esEntityFactory = new ESEntityFactory();

        private static readonly string IndexNamePersons = "persons";
        private static readonly string IndexNameTenures = "tenures";
        private static readonly string IndexNameAssets = "assets";
        private static readonly string IndexNameTransactions = "transactions";
        private static readonly string IndexNameProcesses = "processes";
        private static readonly Dictionary<string, string> _indexes = new Dictionary<string, string>
        {
            { IndexNamePersons, "data/indexes/personIndex.json" },
            { IndexNameTenures, "data/indexes/tenureIndex.json" },
            { IndexNameAssets, "data/indexes/assetIndex.json" },
            { IndexNameTransactions, "data/indexes/transactionIndex.json" },
            { IndexNameProcesses, "data/indexes/processIndex.json" }
        };

        public QueryableAsset AssetInIndex { get; private set; }
        private const string DateFormat = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";

        public ElasticSearchFixture()
        {
            EnsureEnvVarConfigured("ELASTICSEARCH_DOMAIN_URL", "http://localhost:9200");

            _factory = new MockApplicationFactory();
            _host = _factory.CreateHostBuilder(null).Build();
            _host.Start();

            WaitForESInstance(ElasticSearchClient);
            EnsureIndexesExist(ElasticSearchClient);

            LogCallAspectFixture.SetupLogCallAspect();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != _host)
                {
                    _host.StopAsync().GetAwaiter().GetResult();
                    _host.Dispose();
                }
                _disposed = true;
            }
        }

        private static void EnsureIndexesExist(IElasticClient elasticSearchClient)
        {
            foreach (var index in _indexes)
            {
                elasticSearchClient.Indices.Delete(index.Key);

                var indexDoc = File.ReadAllTextAsync(index.Value).Result;
                elasticSearchClient.LowLevel.Indices.CreateAsync<BytesResponse>(index.Key, indexDoc)
                                                    .ConfigureAwait(true);
            }
        }

        private static void WaitForESInstance(IElasticClient elasticSearchClient)
        {
            var esNodes = string.Join(';', elasticSearchClient.ConnectionSettings.ConnectionPool.Nodes.Select(x => x.Uri));
            Console.WriteLine($"ElasticSearch client using {esNodes}");

            Exception ex = null;
            var timeout = DateTime.UtcNow.AddSeconds(5); // 5 second timeout (make configurable?)
            while (DateTime.UtcNow < timeout)
            {
                try
                {
                    var pingResponse = elasticSearchClient.Ping();
                    if (pingResponse.IsValid)
                        return;
                    else
                        ex = pingResponse.OriginalException;
                }
                catch (Exception e)
                {
                    ex = e;
                }

                Thread.Sleep(200);
            }

            if (ex != null)
                throw ex;
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }

        public void GivenAPersonIsNotIndexed(Person person)
        {
            // Nothing to do here
        }
        public void GivenATenureIsNotIndexed(TenureInformation tenure)
        {
            // Nothing to do here
        }

        public async Task GivenAPersonIsIndexedWithDifferentInfo(Person person)
        {
            var esPerson = _esEntityFactory.CreatePerson(person);
            esPerson.Firstname = "Old";
            esPerson.Surname = "Macdonald";
            esPerson.Tenures = new List<QueryablePersonTenure>();
            var request = new IndexRequest<QueryablePerson>(esPerson, IndexNamePersons);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
        }

        public async Task GivenATenureIsIndexed(TenureInformation tenure)
        {
            var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);
            var request = new IndexRequest<QueryableTenure>(esTenure, IndexNameTenures);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
        }
        public async Task GivenATransactionIsIndexed(TransactionResponseObject transaction)
        {
            var esTransaction = _esEntityFactory.CreateQueryableTransaction(transaction);
            var request = new IndexRequest<QueryableTransaction>(esTransaction, IndexNameTransactions);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
        }
        public async Task GivenATenureIsIndexedWithDifferentInfo(TenureInformation tenure)
        {
            var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);
            esTenure.EndOfTenureDate = null;
            esTenure.PaymentReference = null;
            esTenure.TenuredAsset.FullAddress = "Somewhere";
            esTenure.TempAccommodationInfo.BookingStatus = "Some booking status";
            var request = new IndexRequest<QueryableTenure>(esTenure, IndexNameTenures);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
        }

        public void GivenAnAssetIsNotIndexed(string assetId)
        {
            // Nothing to do here
        }

        public async Task GivenAnAssetIsIndexed(string assetId)
        {
            await GivenAnAssetIsIndexed(assetId, Guid.NewGuid().ToString());
        }
        public async Task GivenAnAssetIsIndexed(string assetId, string tenureId)
        {
            var esAssetTenure = _fixture.Build<QueryableAssetTenure>()
                                        .With(x => x.Id, tenureId)
                                        .Create();
            var esAsset = _fixture.Build<QueryableAsset>()
                                  .With(x => x.Id, assetId)
                                  .With(x => x.AssetId, assetId)
                                  .With(x => x.Tenure, esAssetTenure)
                                  .Without(x => x.AssetContracts)
                                  .Create();
            var request = new IndexRequest<QueryableAsset>(esAsset, IndexNameAssets);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
            AssetInIndex = esAsset;
        }

        public async Task GivenTenurePersonsAreIndexed(TenureInformation tenure)
        {
            await GivenTenurePersonsAreIndexed(tenure, false);
        }

        public async Task GivenTenurePersonsAreIndexed(TenureInformation tenure, bool areOld)
        {
            var thisPersonTenure = _fixture.Build<QueryablePersonTenure>()
                                      .With(x => x.AssetFullAddress, tenure.TenuredAsset.FullAddress)
                                      .With(x => x.EndDate, areOld ? null : tenure.EndOfTenureDate)
                                      .With(x => x.Id, tenure.Id)
                                      .With(x => x.PaymentReference, areOld ? null : tenure.PaymentReference)
                                      .With(x => x.StartDate, areOld ? null : tenure.StartOfTenureDate)
                                      .With(x => x.Type, tenure.TenureType.Description)
                                      .Create();
            foreach (var hm in tenure.HouseholdMembers)
            {
                var personTenures = _fixture.CreateMany<QueryablePersonTenure>(2).ToList();
                personTenures.Add(thisPersonTenure);
                var esPerson = _fixture.Build<QueryablePerson>()
                                      .With(x => x.Id, hm.Id)
                                      .With(x => x.DateOfBirth, hm.DateOfBirth)
                                      .With(x => x.Tenures, personTenures)
                                      .Create();

                var request = new IndexRequest<QueryablePerson>(esPerson, IndexNamePersons);
                await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
            }
        }

        public async Task GivenTheOtherPersonTenuresExist(Person person, Guid tenureId)
        {
            for (int i = 0; i < person.Tenures.Count; i++)
            {
                var personTenure = person.Tenures[i];
                var personType = person.PersonTypes[i];
                var esTenure = CreateQueryableTenureForPerson(personTenure.Id, person.Id, personType);

                var request = new IndexRequest<QueryableTenure>(esTenure, IndexNameTenures);
                await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
            }
        }

        private QueryableTenureType ToQueryable(TenureType tt)
        {
            return new QueryableTenureType { Code = tt.Code, Description = tt.Description };
        }

        private QueryableTenure CreateQueryableTenureForPerson(string tenureId, string personId, string personType)
        {
            QueryableTenureType tt;
            bool isResponsible;
            switch (personType)
            {
                case "HouseholderMember":
                    tt = ToQueryable(TenureTypes.Secure);
                    isResponsible = false;
                    break;
                case "Freeholder":
                    tt = ToQueryable(TenureTypes.Freehold);
                    isResponsible = true;
                    break;
                default:
                    tt = ToQueryable(TenureTypes.Secure);
                    isResponsible = true;
                    break;
            }
            var hms = _fixture.Build<QueryableHouseholdMember>()
                              .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-40).ToString(DateFormat))
                              .With(x => x.PersonTenureType, personType)
                              .With(x => x.IsResponsible, isResponsible)
                              .CreateMany(3).ToList();
            hms.Last().Id = personId;

            return _fixture.Build<QueryableTenure>()
                           .With(x => x.Id, tenureId)
                           .With(x => x.TenureType, tt)
                           .With(x => x.HouseholdMembers, hms)
                           .Create();
        }

        public void GivenTheProcessIsNotIndexed(Guid processId)
        {
            // Do Nothing
        }

        public async Task GivenTheProcessIsIndexed(Guid id)
        {
            var process = _fixture.Build<QueryableProcess>()
                                  .With(x => x.Id, id.ToString())
                                  .With(x => x.ProcessStartedAt, _fixture.Create<DateTime>().ToString("O"))
                                  .With(x => x.StateStartedAt, _fixture.Create<DateTime>().ToString("O"))
                                  .Create();
            var request = new IndexRequest<QueryableProcess>(process, IndexNameProcesses);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
        }

        public async Task GivenAContractIsIndexed(string assetId, string contractId)
        {
            var esAssetContracts = _fixture.Build<QueryableAssetContract>()
                                        .With(x => x.Id, contractId)
                                        .CreateMany(1);
            var esAsset = _fixture.Build<QueryableAsset>()
                                  .With(x => x.Id, assetId)
                                  .With(x => x.AssetId, assetId)
                                  .With(x => x.AssetContracts, esAssetContracts)
                                  .Create();
            var request = new IndexRequest<QueryableAsset>(esAsset, IndexNameAssets);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
            AssetInIndex = esAsset;
        }
    }

    [CollectionDefinition("ElasticSearch collection", DisableParallelization = true)]
    public class ElasticSearchCollection : ICollectionFixture<ElasticSearchFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
