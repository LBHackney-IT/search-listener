using System;
using System.Diagnostics.CodeAnalysis;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Elasticsearch.Net;
using Hackney.Core.Logging;
using HousingSearchListener.Gateways;
using HousingSearchListener.V1.HealthCheck;
using HousingSearchListener.V1.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Nest;

namespace HousingSearchListener.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseFunction
    {
        protected IConfigurationRoot Configuration { get; }

        protected IServiceProvider ServiceProvider { get; }

        protected ILogger Logger { get; }

        protected BaseFunction(IServiceCollection services)
        {
            AWSSDKHandler.RegisterXRayForAllServices();

            var builder = new ConfigurationBuilder();

            Configuration = builder.Build();
            RegisterDependencies(services);

            AddLogging(services);
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.UseLogCall();

            Logger = ServiceProvider.GetRequiredService<ILogger<BaseFunction>>();
        }

        private void RegisterDependencies(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddScoped<IPersonMessageFactory, PersonMessageFactory>();
            services.AddScoped<IESPersonFactory, EsPersonFactory>();
            services.AddScoped<IElasticSearchHelper, ElasticSearchHelper>();

            ConfigureElasticsearch(services);
        }

        private void ConfigureElasticsearch(IServiceCollection services)
        {
            var url = Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL") ?? "http://localhost:9200";
            var pool = new SingleNodeConnectionPool(new Uri(url));
            var connectionSettings =
                new ConnectionSettings(pool)
                    .PrettyJson().ThrowExceptions().DisableDirectStreaming();
            var esClient = new ElasticClient(connectionSettings);

            services.TryAddSingleton<IElasticClient>(esClient);

            services.AddElasticSearchHealthCheck();
        }

        private void AddLogging(IServiceCollection services)
        {
            services.ConfigureLambdaLogging(Configuration);
            services.AddLogCallAspect();
        }

        /// <summary>>
        /// Base implementation
        /// Automatically adds LogCallAspect
        /// </summary>
        /// <param name="services"></param>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddLogCallAspect();
        }
    }
}