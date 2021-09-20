using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.UseCase;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Collections.Generic;

namespace HousingSearchListener.V1.Factories
{
    public class MessageHandlerFactory
    {
        private readonly Dictionary<EventTypes, Type> _eventServices;
        private readonly IServiceProvider _services;

        public MessageHandlerFactory(IServiceProvider services)
        {
            _services = services;

            _eventServices = new Dictionary<EventTypes, Type>(6)
            {
                { EventTypes.PersonCreatedEvent, typeof(IndexPersonUseCase) },
                { EventTypes.PersonUpdatedEvent, typeof(IndexPersonUseCase) },
                { EventTypes.TenureCreatedEvent, typeof(IndexTenureUseCase) },
                { EventTypes.AccountCreatedEvent, typeof(AccountAddUseCase) },
                { EventTypes.AccountUpdatedEvent, typeof(AccountUpdateUseCase) }
            };
        }

        public IMessageProcessing ToMessageProcessor(EventTypes eventType)
        {
            if (!_eventServices.TryGetValue(eventType, out Type processorType))
            {
                throw new ArgumentException($"The requested service for the {eventType} was not found.");
            }

            var processor = _services.GetService(processorType);
            if (processor == null || !(processor is IMessageProcessing))
            {
                throw new ArgumentException($"The service with the type {processorType} cannot be created.");
            }

            return processor as IMessageProcessing;
        }
    }
}
