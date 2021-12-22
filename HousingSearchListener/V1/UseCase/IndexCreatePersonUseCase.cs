using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Factories.Interfaces;

namespace HousingSearchListener.V1.UseCase
{
    public class IndexCreatePersonUseCase : IIndexCreatePersonUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IPersonApiGateway _personApiGateway;
        private readonly IPersonFactory _personFactory;

        public IndexCreatePersonUseCase(IEsGateway esGateway, IPersonApiGateway personApiGateway,
            IPersonFactory personFactory)
        {
            _esGateway = esGateway;
            _personApiGateway = personApiGateway;
            _personFactory = personFactory;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Person from Person service API
            var person = await _personApiGateway.GetPersonByIdAsync(message.EntityId, message.CorrelationId)
                                         .ConfigureAwait(false);
            if (person is null) throw new EntityNotFoundException<Person>(message.EntityId);

            // 2. Update the ES index
            var esPerson = _personFactory.CreatePerson(person);
            await _esGateway.IndexPerson(esPerson);
        }
    }
}