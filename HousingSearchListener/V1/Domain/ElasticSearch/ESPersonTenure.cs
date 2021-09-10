namespace HousingSearchListener.V1.Domain.ElasticSearch
{
    public class ESPersonTenure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string AssetFullAddress { get; set; }
        public string PostCode { get; set; }
        public string PaymentReference { get; set; }
        public double TotalBalance { get; set; }
    }
}
