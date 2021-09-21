using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hackney.Core.Logging;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
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

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Person from Person service API
            var person = await _personApiGateway.GetPersonByIdAsync(message.EntityId)
                .ConfigureAwait(false);
            if (person is null) throw new EntityNotFoundException<Person>(message.EntityId);

            // 2. Update the ES index
            var esPerson = _esEntityFactory.CreatePerson(person);
            await _esGateway.IndexPerson(esPerson);

            ////3.  Get tenures for person
            //var listOfTenureTasks = new List<Task<Domain.Tenure.TenureInformation>>();
            //foreach (var tenure in person.Tenures)
            //{
            //    listOfTenureTasks.Add(_tenureApiGateway.GetTenureByIdAsync(new Guid(tenure.Id)));
            //}

            //await Task.WhenAll(listOfTenureTasks).ConfigureAwait(false);

            //var listOfTenures = new List<TenureInformation>();

            //foreach (var tenureTask in listOfTenureTasks)
            //{
            //    listOfTenures.Add(tenureTask.Result);
            //}

            //var listOfUpdateTenureIndexTasks = new List<Task<Nest.IndexResponse>>();

            ////3.  Update each tenure and reindex
            //foreach (var tenure in listOfTenures)
            //{
            //    var householdMember = tenure.HouseholdMembers.SingleOrDefault(x =>
            //        x.Id == person.Id);

            //    if (householdMember != null)
            //    {
            //        householdMember.FullName = person.FullName;
            //        householdMember.DateOfBirth = person.DateOfBirth;

            //        var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);

            //        listOfUpdateTenureIndexTasks.Add(_esGateway.IndexTenure(esTenure));
            //    }
            //}

            ////4.  Wait for all tenures to update
            //await Task.WhenAll(listOfUpdateTenureIndexTasks);
        }
    }
}