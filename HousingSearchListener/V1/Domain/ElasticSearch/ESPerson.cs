using System.Collections.Generic;

namespace HousingSearchListener.V1.Domain.ElasticSearch
{
    public class ESPerson
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Firstname { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string PreferredFirstname { get; set; }
        public string PreferredSurname { get; set; }
        public string DateOfBirth { get; set; }
        public List<ESIdentification> Identifications { get; set; }
        public List<string> PersonTypes { get; set; }
        public bool IsPersonCautionaryAlerted { get; set; }
        public bool IsTenureCautionaryAlerted { get; set; }
        public List<ESPersonTenure> Tenures { get; set; }
    }
}
