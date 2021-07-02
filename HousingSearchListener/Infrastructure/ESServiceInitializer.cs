using System;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nest;

namespace HousingSearchListener.Infrastructure
{
    public static class ESServiceInitializer
    {
        public static void Initialize(IServiceCollection services)
        {
            var url = Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL") ?? "http://localhost:9200";
            var pool = new SingleNodeConnectionPool(new Uri(url));
            var connectionSettings =
                new ConnectionSettings(pool)
                    .PrettyJson().ThrowExceptions().DisableDirectStreaming();
            var esClient = new ElasticClient(connectionSettings);

            services.TryAddSingleton<IElasticClient>(esClient);
        }
    }
}