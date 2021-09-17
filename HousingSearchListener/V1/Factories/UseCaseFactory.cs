﻿using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.UseCase.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HousingSearchListener.V1.Factories
{
    public static class UseCaseFactory
    {
        public static IMessageProcessing CreateUseCaseForMessage(this EntityEventSns entityEvent, IServiceProvider serviceProvider)
        {
            if (entityEvent is null) throw new ArgumentNullException(nameof(entityEvent));
            if (serviceProvider is null) throw new ArgumentNullException(nameof(serviceProvider));

            IMessageProcessing processor = null;
            switch (entityEvent.EventType)
            {
                case EventTypes.PersonCreatedEvent:
                case EventTypes.PersonUpdatedEvent:
                    {
                        processor = serviceProvider.GetService<IIndexPersonUseCase>();
                        break;
                    }
                case EventTypes.TenureCreatedEvent:
                case EventTypes.TenureUpdatedEvent:
                    {
                        processor = serviceProvider.GetService<IIndexTenureUseCase>();
                        break;
                    }
                case EventTypes.PersonAddedToTenureEvent:
                    {
                        processor = serviceProvider.GetService<IAddPersonToTenureUseCase>();
                        break;
                    }

                default:
                    throw new ArgumentException($"Unknown event type: {entityEvent.EventType}");
            }

            return processor;
        }
    }
}