using Hackney.Core.Http;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using HousingSearchListener.V1.Factories.Interfaces;
using HousingSearchListener.V1.Gateway.Interfaces;
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
        private readonly IPersonFactory _personFactory;
        private readonly ITenuresFactory _tenuresFactory;

        public AddPersonToTenureUseCase(IEsGateway esGateway, ITenureApiGateway tenureApiGateway,
            IPersonApiGateway personApiGateway, IPersonFactory personFactory, ITenuresFactory tenuresFactory)
        {
            _esGateway = esGateway;
            _tenureApiGateway = tenureApiGateway;
            _personApiGateway = personApiGateway;
            _personFactory = personFactory;
            _tenuresFactory = tenuresFactory;
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
            var householdMember = GetAddedOrUpdatedHouseholdMember(message.EventData);
            var personId = Guid.Parse(householdMember.Id);

            // 3. Get Added person from Person service API
            var person = await _personApiGateway.GetPersonByIdAsync(personId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (person is null) throw new EntityNotFoundException<Person>(personId);

            UpdatePersonTenures(person, tenure);
            UpdatePersonTypes(person, tenure.TenureType, householdMember.IsResponsible);

            // 4. Update the indexes
            await UpdateTenureIndexAsync(tenure);
            await UpdatePersonIndexAsync(person);
        }

        private void UpdatePersonTenures(Person person, TenureInformation tenure)
        {
            var personTenure = person.Tenures.FirstOrDefault(x => x.Id == tenure.Id);
            if (personTenure is null)
            {
                personTenure = new Tenure();
                person.Tenures.Add(personTenure);
            }
            personTenure.AssetFullAddress = tenure.TenuredAsset.FullAddress;
            personTenure.AssetId = tenure.TenuredAsset.Id;
            personTenure.EndDate = tenure.EndOfTenureDate;
            personTenure.Id = tenure.Id;
            personTenure.IsActive = tenure.IsActive;
            personTenure.StartDate = tenure.StartOfTenureDate;
            personTenure.Type = tenure.TenureType.Description;
            personTenure.Uprn = tenure.TenuredAsset.Uprn;
        }

        private void UpdatePersonTypes(Person person, TenureType tenureType, bool isResponsible)
        {
            var personTenureType = TenureTypes.GetPersonTenureType(tenureType.Code, isResponsible);
            if (!person.PersonTypes.Contains(personTenureType))
                person.PersonTypes.Add(personTenureType);
        }

        private async Task UpdateTenureIndexAsync(TenureInformation tenure)
        {
            var esTenure = _tenuresFactory.CreateQueryableTenure(tenure);
            await _esGateway.IndexTenure(esTenure);
        }

        private async Task UpdatePersonIndexAsync(Person person)
        {
            var esPerson = _personFactory.CreatePerson(person);
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
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj), JsonOptions.Create());
        }
    }
}
