using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.Infrastructure;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.UseCase
{
    public class RemovePersonFromTenureUseCase : IRemovePersonFromTenureUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly ITenureApiGateway _tenureApiGateway;
        private readonly IPersonApiGateway _personApiGateway;
        private readonly IESEntityFactory _esEntityFactory;

        public RemovePersonFromTenureUseCase(IEsGateway esGateway, ITenureApiGateway tenureApiGateway,
            IPersonApiGateway personApiGateway, IESEntityFactory esEntityFactory)
        {
            _esGateway = esGateway;
            _tenureApiGateway = tenureApiGateway;
            _personApiGateway = personApiGateway;
            _esEntityFactory = esEntityFactory;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Tenure from Tenure service API
            var tenure = await _tenureApiGateway.GetTenureByIdAsync(message.EntityId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (tenure is null) throw new EntityNotFoundException<TenureInformation>(message.EntityId);

            // 2. Determine the Person id from event data.
            var householdMember = GetRemovedHouseholdMember(message.EventData);
            var personId = Guid.Parse(householdMember.Id);

            // 3. Get Removed person from Person service API
            var person = await _personApiGateway.GetPersonByIdAsync(personId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (person is null) throw new EntityNotFoundException<Person>(personId);

            // 4. Make sure the tenure is no longer on the person
            if (person.Tenures.Any(x => x.Id == tenure.Id))
                person.Tenures.Remove(person.Tenures.First(x => x.Id == tenure.Id));

            // 5. Update the person.PersonType list if necessary
            UpdatePersonType(person);

            // 6. Update the indexes
            await UpdateTenureIndexAsync(tenure).ConfigureAwait(false);
            await UpdatePersonIndexAsync(person).ConfigureAwait(false);
        }

        private void UpdatePersonType(Person person)
        {
            var getTenureFromIndexTasks = person.Tenures.Select(x => _esGateway.GetTenureById(x.Id)).ToArray();
            Task.WaitAll(getTenureFromIndexTasks);

            var personTypes = getTenureFromIndexTasks.Select(x => GetPersonTypeForTenure(x.Result, person.Id)).ToList();
            person.PersonTypes = personTypes;
        }

        private string GetPersonTypeForTenure(QueryableTenure tenure, string personId)
        {
            var hm = tenure.HouseholdMembers.First(x => x.Id == personId);
            return TenureTypes.GetPersonTenureType(tenure.TenureType.Code, hm.IsResponsible);
        }

        private async Task UpdateTenureIndexAsync(TenureInformation tenure)
        {
            var esTenure = _esEntityFactory.CreateQueryableTenure(tenure);
            await _esGateway.IndexTenure(esTenure);
        }

        private async Task UpdatePersonIndexAsync(Person person)
        {
            var esPerson = _esEntityFactory.CreatePerson(person);
            await _esGateway.IndexPerson(esPerson);
        }

        private static HouseholdMembers GetRemovedHouseholdMember(EventData eventData)
        {
            var oldHms = GetHouseholdMembersFromEventData(eventData.OldData);
            var newHms = GetHouseholdMembersFromEventData(eventData.NewData);

            return oldHms.Except(newHms).FirstOrDefault();
        }

        private static List<HouseholdMembers> GetHouseholdMembersFromEventData(object data)
        {
            var dataDic = (data is Dictionary<string, object>) ? data as Dictionary<string, object> : ConvertFromObject<Dictionary<string, object>>(data);
            var hmsObj = dataDic["householdMembers"];
            return (hmsObj is List<HouseholdMembers>) ? hmsObj as List<HouseholdMembers> : ConvertFromObject<List<HouseholdMembers>>(hmsObj);
        }

        private static T ConvertFromObject<T>(object obj) where T : class
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj), JsonOptions.CreateJsonOptions());
        }
    }
}
