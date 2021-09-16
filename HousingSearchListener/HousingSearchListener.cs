using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Hackney.Core.Logging;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure;
using HousingSearchListener.V1.UseCase;
using HousingSearchListener.V1.UseCase.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HousingSearchListener
{
    [ExcludeFromCodeCoverage]
    public class HousingSearchListener : BaseFunction
    {
        private readonly static JsonSerializerOptions _jsonOptions = JsonOptions.CreateJsonOptions();

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public HousingSearchListener()
        {
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddScoped<IESEntityFactory, ESEntityFactory>();
            services.AddScoped<IEsGateway, EsGateway>();
            services.AddScoped<IPersonApiGateway, PersonApiGateway>();
            services.AddScoped<ITenureApiGateway, TenureApiGateway>();
            services.AddScoped<IAccountApiGateway, AccountApiGateway>();

            services.ConfigureElasticSearch(Configuration);

            services.AddScoped<MessageHandlerFactory>();

            services.AddScoped<IndexPersonUseCase>();
            services.AddScoped<IndexTenureUseCase>();
            services.AddScoped<AccountUpdateUseCase>();
            services.AddScoped<AccountAddUseCase>();
            services.AddScoped<PersonBalanceUpdatedUseCase>();

            base.ConfigureServices(services);
        }

        public async Task FunctionHandler(SQSEvent snsEvent, ILambdaContext context)
        {
            // Do this in parallel???
            foreach (var message in snsEvent.Records)
            {
                await ProcessMessageAsync(message, context).ConfigureAwait(false);
            }
        }

        [LogCall(LogLevel.Information)]
        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processing message {message.MessageId}");

            var entityEvent = JsonSerializer.Deserialize<EntityEventSns>(message.Body, _jsonOptions);

            using (Logger.BeginScope("CorrelationId: {CorrelationId}", entityEvent.CorrelationId))
            {
                try
                {
                    if (!Enum.TryParse(entityEvent.EventType, out EventTypes eventType))
                    {
                        throw new Exception($"The {eventType} does not exist.");
                    }

                    IMessageProcessing processor = GetMessageHandlerFactory().ToMessageProcessor(eventType);
                    await processor.ProcessMessageAsync(entityEvent).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Exception processing message id: {message.MessageId}; type: {entityEvent.EventType}; entity id: {entityEvent.EntityId}");
                    throw; // AWS will handle retry/moving to the dead letter queue
                }
            }
        }

        private MessageHandlerFactory GetMessageHandlerFactory() 
            => ServiceProvider.GetService<MessageHandlerFactory>();
    }
}