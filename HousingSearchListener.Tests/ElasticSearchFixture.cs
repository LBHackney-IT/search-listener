using AutoFixture;
using Elasticsearch.Net;
using HousingSearchListener.V1.Domain.ElasticSearch.Asset;
using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
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
using Xunit;
using QueryableTenure = HousingSearchListener.V1.Domain.ElasticSearch.Tenure.QueryableTenure;

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
        private static readonly Dictionary<string, string> _indexes = new Dictionary<string, string>
        {
            { IndexNamePersons, "data/indexes/personIndex.json" },
            { IndexNameTenures, "data/indexes/tenureIndex.json" },
            { IndexNameAssets, "data/indexes/assetIndex.json" }
        };

        public QueryableAsset AssetInIndex { get; private set; }

        public ElasticSearchFixture()
        {
            EnsureEnvVarConfigured("ELASTICSEARCH_DOMAIN_URL", "http://localhost:9200");

            EnsureEnvVarConfigured("PersonApiUrl", FixtureConstants.PersonApiRoute);
            EnsureEnvVarConfigured("PersonApiToken", FixtureConstants.PersonApiToken);
            EnsureEnvVarConfigured("TenureApiUrl", FixtureConstants.TenureApiRoute);
            EnsureEnvVarConfigured("TenureApiToken", FixtureConstants.TenureApiToken);

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
            esPerson.Tenures = new List<ESPersonTenure>();
            var request = new IndexRequest<ESPerson>(esPerson, IndexNamePersons);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
        }

        public async Task GivenATenureIsIndexed(TenureInformation tenure)
        {
            var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);
            var request = new IndexRequest<QueryableTenure>(esTenure, IndexNameTenures);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
        }

        public async Task GivenATenureIsIndexedWithDifferentInfo(TenureInformation tenure)
        {
            var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);
            esTenure.EndOfTenureDate = null;
            esTenure.PaymentReference = null;
            esTenure.TenuredAsset.FullAddress = "Somewhere";
            var request = new IndexRequest<QueryableTenure>(esTenure, IndexNameTenures);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
        }

        public void GivenAnAssetIsNotIndexed(string assetId)
        {
            // Nothing to do here
        }

        public async Task GivenAnAssetIsIndexed(string assetId)
        {
            var esAsset = _fixture.Build<QueryableAsset>()
                                  .With(x => x.Id, assetId)
                                  .With(x => x.AssetId, assetId)
                                  .Create();
            var request = new IndexRequest<QueryableAsset>(esAsset, IndexNameAssets);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
            AssetInIndex = esAsset;
        }

        public async Task GivenATenureIsIndexedWithDifferentInfo(TenureInformation tenure)
        {
            var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);
            esTenure.EndOfTenureDate = null;
            esTenure.PaymentReference = null;
            esTenure.TenuredAsset.FullAddress = "Somewhere";
            var request = new IndexRequest<QueryableTenure>(esTenure, IndexNameTenures);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
        }

        public void GivenAnAssetIsNotIndexed(string assetId)
        {
            // Nothing to do here
        }

        public async Task GivenAnAssetIsIndexed(string assetId)
        {
            var esAsset = _fixture.Build<QueryableAsset>()
                                  .With(x => x.Id, assetId)
                                  .With(x => x.AssetId, assetId)
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
