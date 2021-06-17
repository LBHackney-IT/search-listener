using System.Collections.Generic;
using HousingSearchListener.V1.Domain;
using Nest;

namespace HousingSearchListener.Tests.E2E
{
    public class QueryablePerson
    {
        [Text(Name = "id")]
        public string Id { get; set; }
        public string Title { get; set; }

        [Keyword(Name = "firstname")]
        public string Firstname { get; set; }

        [Text(Name = "middlename")]
        public string MiddleName { get; set; }

        [Keyword(Name = "surname")]
        public string Surname { get; set; }

        [Text(Name = "preferredFirstname")]
        public string PreferredFirstname { get; set; }

        [Text(Name = "preferredSurname")]
        public string PreferredSurname { get; set; }

        public string Ethinicity { get; set; }

        public string Nationality { get; set; }

        public string PlaceOfBirth { get; set; }

        [Text(Name = "dateOfBirth")]
        public string DateOfBirth { get; set; }

        public string Gender { get; set; }

        public List<Identification> Identification { get; set; }

        public List<string> PersonTypes { get; set; }

        public bool IsPersonCautionaryAlert { get; set; }

        public bool IsTenureCautionaryAlert { get; set; }

        public List<Tenure> Tenures { get; set; }

    }
}