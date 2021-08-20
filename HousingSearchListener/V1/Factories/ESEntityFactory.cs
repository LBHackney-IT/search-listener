using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using System.Collections.Generic;
using System.Linq;

namespace HousingSearchListener.V1.Factories
{
    public class ESEntityFactory : IESEntityFactory
    {
        private List<ESIdentification> CreateIdentifications(List<Identification> identifications)
        {
            return identifications.Select(x => new ESIdentification
            {
                IdentificationType = x.IdentificationType,
                IsOriginalDocumentSeen = x.IsOriginalDocumentSeen,
                LinkToDocument = x.LinkToDocument,
                Value = x.Value
            }).ToList();
        }

        private List<ESTenure> CreateTenures(List<Tenure> tenures)
        {
            return tenures.Select(x => new ESTenure
            {
                AssetFullAddress = x.AssetFullAddress,
                EndDate = x.EndDate,
                Id = x.Id,
                StartDate = x.StartDate,
                Type = x.Type
            }).ToList();
        }

        public ESPerson CreatePerson(Person person)
        {
            return new ESPerson
            {
                Id = person.Id,
                DateOfBirth = person.DateOfBirth,
                Title = person.Title,
                Firstname = person.FirstName,
                Surname = person.Surname,
                MiddleName = person.MiddleName,
                PreferredFirstname = person.PreferredFirstName,
                PreferredSurname = person.PreferredSurname,
                Identifications = person.Identifications != null ? CreateIdentifications(person.Identifications) : new List<ESIdentification>(),
                PersonTypes = person.PersonType,
                Tenures = person.Tenures != null ? CreateTenures(person.Tenures) : new List<ESTenure>()
            };
        }

        public ESTenure CreateTenure(TenureInformation tenure)
        {
            return new ESTenure
            {
                Id = tenure.Id,
                AssetFullAddress = tenure.TenuredAsset.FullAddress,
                StartDate = tenure.StartOfTenureDate,
                EndDate = tenure.EndOfTenureDate,
                Type = tenure.TenureType.Description
            };
        }
    }
}