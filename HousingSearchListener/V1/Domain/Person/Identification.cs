namespace HousingSearchListener.V1.Domain.Person
{
    public class Identification
    {
        public string IdentificationType { get; set; }
        public string Value { get; set; }
        public bool IsOriginalDocumentSeen { get; set; }
        public string LinkToDocument { get; set; }
    }
}