using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.UseCase;
using HousingSearchListener.V1.UseCase.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace HousingSearchListener.V1.Factories
{
    public class MessageHandlerFactory
    {
        private readonly Dictionary<EventTypes, IMessageProcessing> _eventServices;
        private readonly IServiceProvider _services;

        public MessageHandlerFactory(IServiceProvider services)
        {
            _services = services;

            _eventServices = new Dictionary<EventTypes, IMessageProcessing>(6)
            {
                { EventTypes.PersonCreatedEvent, _services.GetService<IndexPersonUseCase>() },
                { EventTypes.PersonUpdatedEvent, _services.GetService<IndexPersonUseCase>() },
                { EventTypes.TenureCreatedEvent, _services.GetService<IndexTenureUseCase>() },
                { EventTypes.AccountCreatedEvent, _services.GetService<AccountAddUseCase>() },
                { EventTypes.AccountUpdatedEvent, _services.GetService<AccountUpdateUseCase>() },
                { EventTypes.PersonBalanceUpdatedEvent, _services.GetService<PersonBalanceUpdatedUseCase>() }
            };
        }

        public IMessageProcessing ToMessageProcessor(EventTypes eventType)
        {
            if (!_eventServices.TryGetValue(eventType, out IMessageProcessing processor))
            {
                throw new ArgumentException($"The requested service for the {eventType} was not found.");
            }

            return processor;
        }
    }
}
