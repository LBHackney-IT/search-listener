using System;
using System.Diagnostics.CodeAnalysis;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Hackney.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HousingSearchListener.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseService
    {
        protected IConfigurationRoot Configuration { get; }

        protected IServiceProvider ServiceProvider { get; }

        protected ILogger Logger { get; }

        protected BaseService(IServiceCollection services)
        {
            AWSSDKHandler.RegisterXRayForAllServices();

            var builder = new ConfigurationBuilder();

            Configuration = builder.Build();

            AddLogging(services);
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.UseLogCall();

            Logger = ServiceProvider.GetRequiredService<ILogger<BaseService>>();
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