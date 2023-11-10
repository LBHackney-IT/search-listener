using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Gateway.Interfaces;
using Amazon.Lambda.Core;

namespace HousingSearchListener.V1.UseCase
{
    public class IndexCreatePersonUseCase : IIndexCreatePersonUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IPersonApiGateway _personApiGateway;
        private readonly IESEntityFactory _esPersonFactory;

        public IndexCreatePersonUseCase(IEsGateway esGateway, IPersonApiGateway personApiGateway,
            IESEntityFactory esPersonFactory)
        {
            _esGateway = esGateway;
            _personApiGateway = personApiGateway;
            _esPersonFactory = esPersonFactory;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            LambdaLogger.Log($"{nameof(ProcessMessageAsync)}: id is {message.EntityId}");
            
            // 1. Get Person from Person service API
            var person = await _personApiGateway.GetPersonByIdAsync(message.EntityId, message.CorrelationId)
                                         .ConfigureAwait(false);
            if (person is null) throw new EntityNotFoundException<Person>(message.EntityId);

            LambdaLogger.Log($"{nameof(ProcessMessageAsync)}: person is {person}");

            // 2. Update the ES index
            var esPerson = _esPersonFactory.CreatePerson(person);
            await _esGateway.IndexPerson(esPerson);
        }
    }
}