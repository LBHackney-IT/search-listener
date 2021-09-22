using FluentAssertions;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.UseCase.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace HousingSearchListener.Tests.V1.Factories
{
    /// <summary>
    /// This test class is created to ensure that handlers for all <see cref="EventTypes"/> enum items are registered and can be resolved  
    /// This will prevent run-time errors if we forgot some registration
    /// </summary>
    public class MessageHandlerFactoryTests : HousingSearchListener
    {
        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
            {
                Environment.SetEnvironmentVariable(name, defaultValue);
            }
        }

        // Hanna Holasava 
        // We need to hide this inherited members because we eant to configure API urls befire creating Configuration object
        protected IConfigurationRoot Configuration { get; }
        protected IServiceProvider ServiceProvider { get; }

        public MessageHandlerFactoryTests()
        {
            var services = new ServiceCollection();
            var builder = new ConfigurationBuilder();

            EnsureEnvVarConfigured("PersonApiUrl", "http://localhost:5000");
            EnsureEnvVarConfigured("PersonApiToken", "PersonApiToken");
            EnsureEnvVarConfigured("TenureApiUrl", "http://localhost:5001");
            EnsureEnvVarConfigured("TenureApiToken", "TenureApiToken");

            // Hanna Holasava
            // We need to repeat configuration here after adding missed API urls to environment configurations
            Configure(builder);
            Configuration = builder.Build();
            services.AddSingleton<IConfiguration>(Configuration);

            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void EnsureAllTypesCanBeResolved()
        {
            MessageHandlerFactory messageHandlerFactory = ServiceProvider.GetService<MessageHandlerFactory>();
            var values = Enum.GetValues(typeof(EventTypes)).Cast<EventTypes>();

            foreach (var eventType in values)
            {
                try
                {
                    IMessageProcessing processor = messageHandlerFactory.ToMessageProcessor(eventType);

                    processor.Should().NotBeNull($"The event handler use case for the event [{eventType}] cannot be created.");
                }
                catch (Exception ex)
                {
                    Assert.True(false, $"The event handler use case for the event [{eventType}] cannot be created. The reason: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                }
            }
        }
    }
}
