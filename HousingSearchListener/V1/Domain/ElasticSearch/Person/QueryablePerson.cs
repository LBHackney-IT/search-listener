using Nest;
using System.Collections.Generic;

namespace HousingSearchListener.V1.Domain.ElasticSearch.Person
{
    public class QueryablePerson
    {
        [Text(Name = "id")]
        public string Id { get; set; }

        [Text(Name = "title")]
        public string Title { get; set; }

        [Keyword(Name = "firstname")]
        public string Firstname { get; set; }

        [Text(Name = "middleName")]
        public string MiddleName { get; set; }

        [Keyword(Name = "surname")]
        public string Surname { get; set; }

        [Text(Name = "preferredFirstname")]
        public string PreferredFirstname { get; set; }

        [Text(Name = "preferredSurname")]
        public string PreferredSurname { get; set; }

        [Text(Name = "totalBalance")]
        public decimal TotalBalance { get; set; }

        [Text(Name = "ethinicity")]
        public string Ethinicity { get; set; }

        [Text(Name = "nationality")]
        public string Nationality { get; set; }

        [Text(Name = "placeOfBirth")]
        public string PlaceOfBirth { get; set; }

        [Text(Name = "dateOfBirth")]
        public string DateOfBirth { get; set; }

        [Text(Name = "gender")]
        public string Gender { get; set; }

        [Text(Name = "identification")]
        public List<QueryableIdentification> Identification { get; set; }

        [Text(Name = "personTypes")]
        public List<string> PersonTypes { get; set; }

        [Text(Name = "isPersonCautionaryAlert")]
        public bool IsPersonCautionaryAlert { get; set; }

        [Text(Name = "isTenureCautionaryAlert")]
        public bool IsTenureCautionaryAlert { get; set; }

        [Text(Name = "tenures")]
        public List<QueryablePersonTenure> Tenures { get; set; }
    }
}
