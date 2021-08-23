using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Hackney.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HousingSearchListener
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseFunction
    {
        protected IConfigurationRoot Configuration { get; }
        protected IServiceProvider ServiceProvider { get; }
        protected ILogger Logger { get; }

        internal BaseFunction()
        {
            AWSSDKHandler.RegisterXRayForAllServices();

            var services = new ServiceCollection();
            var builder = new ConfigurationBuilder();

            Configure(builder);
            Configuration = builder.Build();
            services.AddSingleton<IConfiguration>(Configuration);

            services.ConfigureLambdaLogging(Configuration);
            services.AddLogCallAspect();

            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.UseLogCall();

            Logger = ServiceProvider.GetRequiredService<ILogger<BaseFunction>>();
        }

        /// <summary>
        /// Base implementation
        /// Automatically adds environment variables and the appsettings file
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void Configure(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json");
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            if (!string.IsNullOrEmpty(environment))
            {
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"appsettings.{environment}.json");
                if (File.Exists(path))
                    builder.AddJsonFile(path);
            }
            builder.AddEnvironmentVariables();
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