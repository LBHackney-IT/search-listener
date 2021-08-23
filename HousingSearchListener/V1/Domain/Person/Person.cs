using System.Collections.Generic;

namespace HousingSearchListener.V1.Domain.Person
{
    public class Person
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string PreferredTitle { get; set; }
        public string PreferredFirstName { get; set; }
        public string PreferredMiddleName { get; set; }
        public string PreferredSurname { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string Ethinicity { get; set; }
        public string Nationality { get; set; }
        public string NationalInsuranceNo { get; set; }
        public string PlaceOfBirth { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public List<Identification> Identifications { get; set; }
        public List<LanguageClass> Languages { get; set; }
        public List<string> CommunicationRequirements { get; set; }
        public List<string> PersonType { get; set; }
        public List<Tenure> Tenures { get; set; }
        public List<Link> Links { get; set; }
    }
}