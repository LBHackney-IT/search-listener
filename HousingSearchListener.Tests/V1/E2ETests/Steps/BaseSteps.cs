using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using AutoFixture;
using FluentAssertions;
using Hackney.Core.Http;
using Hackney.Core.Sns;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.E2ETests.Steps
{
    public class BaseSteps
    {
        protected readonly JsonSerializerOptions _jsonOptions = JsonOptions.Create();
        protected readonly Fixture _fixture = new Fixture();
        protected Exception _lastException;
        protected string _eventType;
        protected readonly Guid _correlationId = Guid.NewGuid();
        protected readonly List<Action> _cleanup = new List<Action>();

        public BaseSteps()
        { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var action in _cleanup)
                    action();

                _disposed = true;
            }
        }


        protected EntityEventSns CreateEvent(Guid entityId, string eventType, string targetId = null)
        {
            var EntityEventBuilder = _fixture.Build<EntityEventSns>()
                                .With(x => x.EntityId, entityId)
                                .With(x => x.EventType, eventType)
                                .With(x => x.CorrelationId, _correlationId);

            //had to make this conditional as it was messing with preexisting tests
            if (targetId != null)
            {
                EntityEventBuilder = EntityEventBuilder.With(x => x.EventData, new EventData
                {
                    NewData = new { Id = targetId }
                });
            }

            return EntityEventBuilder.Create();
        }

        protected SQSEvent.SQSMessage CreateMessage(Guid personId)
        {
            return CreateMessage(CreateEvent(personId, _eventType));
        }

        protected SQSEvent.SQSMessage CreateMessage(EntityEventSns eventSns)
        {
            var msgBody = JsonSerializer.Serialize(eventSns, _jsonOptions);
            return _fixture.Build<SQSEvent.SQSMessage>()
                           .With(x => x.Body, msgBody)
                           .With(x => x.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>())
                           .Create();
        }

        protected async Task TriggerFunction(Guid id)
        {
            await TriggerFunction(CreateMessage(id)).ConfigureAwait(false);
        }

        protected async Task TriggerFunction(SQSEvent.SQSMessage message)
        {
            var mockLambdaLogger = new Mock<ILambdaLogger>();
            ILambdaContext lambdaContext = new TestLambdaContext()
            {
                Logger = mockLambdaLogger.Object
            };

            var sqsEvent = _fixture.Build<SQSEvent>()
                                   .With(x => x.Records, new List<SQSEvent.SQSMessage> { message })
                                   .Create();

            Func<Task> func = async () =>
            {
                var fn = new HousingSearchListener();
                await fn.FunctionHandler(sqsEvent, lambdaContext).ConfigureAwait(false);
            };

            _lastException = await Record.ExceptionAsync(func).ConfigureAwait(false);
        }

        public void ThenTheCorrelationIdWasUsedInTheApiCall(List<string> receivedCorrelationIds)
        {
            receivedCorrelationIds.Should().Contain(_correlationId.ToString());
        }
    }
}
