using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class IndexCreatePersonUseCase : IIndexCreatePersonUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IPersonApiGateway _personApiGateway;
        private readonly IESEntityFactory _esPersonFactory;

        public Person Person { get; set; }

        public IndexCreatePersonUseCase(IEsGateway esGateway, IPersonApiGateway personApiGateway,
            IESEntityFactory esPersonFactory)
        {
            _esGateway = esGateway;
            _personApiGateway = personApiGateway;
            _esPersonFactory = esPersonFactory;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Person from Person service API
            Person = await _personApiGateway.GetPersonByIdAsync(message.EntityId)
                                         .ConfigureAwait(false);
            if (Person is null) throw new EntityNotFoundException<Person>(message.EntityId);

            // 2. Update the ES index
            var esPerson = _esPersonFactory.CreatePerson(Person);
            await _esGateway.IndexPerson(esPerson);
        }
    }
}