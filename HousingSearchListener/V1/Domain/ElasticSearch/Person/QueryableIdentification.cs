namespace HousingSearchListener.V1.Domain.ElasticSearch.Person
{
    public class QueryableIdentification
    {
        public string IdentificationType { get; set; }

        public string Value { get; set; }

        public bool OriginalDocumentSeen { get; set; }

        public string LinkToDocument { get; set; }
    }
}