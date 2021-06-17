using System.Collections.Generic;
using System.Linq;
using HousingSearchListener.V1.Domain;
using HousingSearchListener.V1.Domain.ElasticSearch;

namespace HousingSearchListener.Gateways
{
    public class EsPersonFactory : IESPersonFactory
    {
        public ESPerson Create(Person person)
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
    }
}