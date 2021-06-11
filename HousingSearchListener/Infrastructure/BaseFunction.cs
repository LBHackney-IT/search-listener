using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Hackney.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

[ExcludeFromCodeCoverage]
public abstract class BaseFunction
{
    protected IConfigurationRoot Configuration { get; }

    protected IServiceProvider ServiceProvider { get; }

    protected ILogger Logger { get; }

    protected BaseFunction()
    {
        AWSSDKHandler.RegisterXRayForAllServices();

        var services = new ServiceCollection();
        var builder = new ConfigurationBuilder();

        Configuration = builder.Build();
        
        services.AddSingleton<IConfiguration>(Configuration);

        AddLogging(services);
        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
        ServiceProvider.UseLogCall();

        Logger = ServiceProvider.GetRequiredService<ILogger<BaseFunction>>();
    }

    private void AddLogging(ServiceCollection services)
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