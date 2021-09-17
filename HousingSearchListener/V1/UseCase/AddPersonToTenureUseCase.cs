﻿using HousingSearchListener.V1.Boundary;
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
    public class AddPersonToTenureUseCase : IAddPersonToTenureUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly ITenureApiGateway _tenureApiGateway;
        private readonly IPersonApiGateway _personApiGateway;
        private readonly IESEntityFactory _esEntityFactory;

        public AddPersonToTenureUseCase(IEsGateway esGateway, ITenureApiGateway tenureApiGateway,
            IPersonApiGateway personApiGateway, IESEntityFactory esEntityFactory)
        {
            _esGateway = esGateway;
            _tenureApiGateway = tenureApiGateway;
            _personApiGateway = personApiGateway;
            _esEntityFactory = esEntityFactory;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Tenure from Tenure service API
            var tenure = await _tenureApiGateway.GetTenureByIdAsync(message.EntityId)
                                                .ConfigureAwait(false);
            if (tenure is null) throw new EntityNotFoundException<TenureInformation>(message.EntityId);

            // 2. Determine the Person id from event data.
            var householdMember = GetAddedOrUpdatedHouseholdMember(message.EventData);
            var personId = Guid.Parse(householdMember.Id);

            // 3. Get Added person from Person service API
            var person = await _personApiGateway.GetPersonByIdAsync(personId)
                                                .ConfigureAwait(false);
            if (person is null) throw new EntityNotFoundException<Person>(personId);

            // 4. Update the indexes
            await UpdateTenureIndexAsync(tenure);
            await UpdatePersonIndexAsync(person);
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

        private static HouseholdMembers GetAddedOrUpdatedHouseholdMember(EventData eventData)
        {
            var oldHms = GetHouseholdMembersFromEventData(eventData.OldData);
            var newHms = GetHouseholdMembersFromEventData(eventData.NewData);

            return newHms.Except(oldHms).FirstOrDefault();
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