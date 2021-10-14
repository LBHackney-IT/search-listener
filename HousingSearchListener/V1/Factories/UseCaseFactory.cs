using Hackney.Core.Sns;
using HousingSearchListener.V1.UseCase.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

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
                    {
                        processor = serviceProvider.GetService<IIndexCreatePersonUseCase>();
                        break;
                    }
                case EventTypes.PersonUpdatedEvent:
                    {
                        processor = serviceProvider.GetService<IIndexUpdatePersonUseCase>();
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
                case EventTypes.PersonRemovedFromTenureEvent:
                    {
                        processor = serviceProvider.GetService<IRemovePersonFromTenureUseCase>();
                        break;
                    }

                default:
                    throw new ArgumentException($"Unknown event type: {entityEvent.EventType}");
            }

            return processor;
        }
    }
}
