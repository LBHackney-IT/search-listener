using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;

namespace HousingSearchListener.V1.UseCase
{
    public class IndexUpdatePersonUseCase : IIndexUpdatePersonUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IPersonApiGateway _personApiGateway;
        private readonly ITenureApiGateway _tenureApiGateway;
        private readonly IESEntityFactory _esEntityFactory;

        public IndexUpdatePersonUseCase(IEsGateway esGateway, IPersonApiGateway personApiGateway, ITenureApiGateway tenureApiGateway,
            IESEntityFactory esEntityFactory)
        {
            _esGateway = esGateway;
            _personApiGateway = personApiGateway;
            _tenureApiGateway = tenureApiGateway;
            _esEntityFactory = esEntityFactory;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            // Same as Create Person
            var person = await _personApiGateway.GetPersonByIdAsync(message.EntityId)
                .ConfigureAwait(false);
            if (person is null) throw new EntityNotFoundException<Person>(message.EntityId);

            // Get tenures
            var listOfTenureTasks = new List<Task<Domain.Tenure.TenureInformation>>();
            foreach (var tenure in person.Tenures)
            {
                listOfTenureTasks.Add(_tenureApiGateway.GetTenureByIdAsync(new Guid(tenure.Id)));
            }

            await Task.WhenAll(listOfTenureTasks).ConfigureAwait(false);

            // For each tenure update the right householdmember with the new person details
            foreach (var tenureTask in listOfTenureTasks)
            {
                var tenureInformation = tenureTask.Result;
                var householdMember = tenureInformation.HouseholdMembers.SingleOrDefault(x =>
                    x.Id == person.Id);
                if (householdMember != null)
                {
                    householdMember.Id = person.Id;
                    householdMember.FullName = person.FullName;
                    householdMember.DateOfBirth = person.DateOfBirth;
                }
            }

            var listOfUpdateTenureIndexTasks = new List<Task<Nest.IndexResponse>>();

            foreach (var tenure in listOfTenureTasks.Select(x => x.Result))
            {
                var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);
                listOfUpdateTenureIndexTasks.Add(_esGateway.IndexTenure(esTenure));
            }

            await Task.WhenAll(listOfUpdateTenureIndexTasks);
        }
    }
}