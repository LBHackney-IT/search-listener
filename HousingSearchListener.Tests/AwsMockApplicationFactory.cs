using HousingSearchListener.V1.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;

namespace HousingSearchListener.Tests
{
    public class AwsMockApplicationFactory
    {
        public IElasticClient ElasticSearchClient { get; private set; }

        public AwsMockApplicationFactory()
        {
        }

        public IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
           .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
           .ConfigureServices((hostContext, services) =>
           {
               services.AddSingleton<IConfiguration>(hostContext.Configuration);
               services.ConfigureElasticSearch(hostContext.Configuration);

               var serviceProvider = services.BuildServiceProvider();
               ElasticSearchClient = serviceProvider.GetRequiredService<IElasticClient>();
           });
    }
}
