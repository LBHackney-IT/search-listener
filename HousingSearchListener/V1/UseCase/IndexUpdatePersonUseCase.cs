using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;

namespace HousingSearchListener.V1.UseCase.Interfaces
{
    public class IndexUpdatePersonUseCase : IIndexUpdatePersonUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly ITenureApiGateway _tenureApiGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly IIndexCreatePersonUseCase _createPersonUseCase;

        public IndexUpdatePersonUseCase(IEsGateway esGateway, ITenureApiGateway tenureApiGateway,
            IESEntityFactory esEntityFactory, IIndexCreatePersonUseCase createPersonUseCase)
        {
            _esGateway = esGateway;
            _tenureApiGateway = tenureApiGateway;
            _esEntityFactory = esEntityFactory;
            _createPersonUseCase = createPersonUseCase;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            // Same as Create Person
            await _createPersonUseCase.ProcessMessageAsync(message);

            // Get tenures
            var listOfTenureTasks = new List<Task<Domain.Tenure.TenureInformation>>();
            foreach (var tenure in _createPersonUseCase.Person.Tenures)
            {
                listOfTenureTasks.Add(_tenureApiGateway.GetTenureByIdAsync(new Guid(tenure.Id)));
            }

            await Task.WhenAll(listOfTenureTasks).ConfigureAwait(false);

            // For each tenure update the right householdmember with the new person details
            foreach (var tenureTask in listOfTenureTasks)
            {
                var tenureInformation = tenureTask.Result;
                var householdMember = tenureInformation.HouseholdMembers.SingleOrDefault(x =>
                    x.Id == _createPersonUseCase.Person.Id);
                if (householdMember != null)
                {
                    householdMember.Id = _createPersonUseCase.Person.Id;
                    householdMember.FullName = _createPersonUseCase.Person.FullName;
                    householdMember.DateOfBirth = _createPersonUseCase.Person.DateOfBirth;
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