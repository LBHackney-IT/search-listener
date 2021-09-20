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
        public string PlaceOfBirth { get; set; }
        public string DateOfBirth { get; set; }
        public List<string> PersonType { get; set; }
        public List<Tenure> Tenures { get; set; }

        public string FullName => FormatFullName();

        private string FormatFullName()
        {
            string firstName = FormatNamePart(FirstName);
            string middleName = FormatNamePart(MiddleName);
            string surname = FormatNamePart(Surname);
            return $"{Title}{firstName}{middleName}{surname}";
        }

        private static string FormatNamePart(string part)
        {
            return string.IsNullOrEmpty(part) ? string.Empty : $" {part}";
        }
    }
}