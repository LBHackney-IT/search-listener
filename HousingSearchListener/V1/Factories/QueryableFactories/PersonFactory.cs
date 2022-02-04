using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Factories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace HousingSearchListener.V1.Factories.QueryableFactories
{
    public class PersonFactory : IPersonFactory
    {
        private List<QueryablePersonTenure> CreatePersonTenures(List<V1.Domain.Person.Tenure> tenures)
        {
            return tenures.Select(x => new QueryablePersonTenure
            {
                AssetFullAddress = x.AssetFullAddress,
                EndDate = x.EndDate,
                Id = x.Id,
                StartDate = x.StartDate,
                Type = x.Type
            }).ToList();
        }
        public QueryablePerson CreatePerson(Person person)
        {
            return new QueryablePerson
            {
                Id = person.Id,
                DateOfBirth = person.DateOfBirth,
                Title = person.Title,
                Firstname = person.FirstName,
                Surname = person.Surname,
                Middlename = person.MiddleName,
                PreferredFirstname = person.PreferredFirstName,
                PreferredSurname = person.PreferredSurname,
                PersonTypes = person.PersonTypes,
                Tenures = person.Tenures != null ? CreatePersonTenures(person.Tenures) : new List<QueryablePersonTenure>()
            };
        }
    }
}
