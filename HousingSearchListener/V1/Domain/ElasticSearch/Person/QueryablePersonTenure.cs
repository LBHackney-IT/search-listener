using Nest;

namespace HousingSearchListener.V1.Domain.ElasticSearch.Person
{
    public class QueryablePersonTenure
    {
        [Text(Name = "id")]
        public string Id { get; set; }

        [Text(Name = "type")]
        public string Type { get; set; }

        [Text(Name = "totalBalance")]
        public decimal TotalBalance { get; set; }

        [Text(Name = "startDate")]
        public string StartDate { get; set; }

        [Text(Name = "endDate")]
        public string EndDate { get; set; }

        [Text(Name = "assetFullAddress")]
        public string AssetFullAddress { get; set; }

        [Text(Name = "postCode")]
        public string PostCode { get; set; }

        [Text(Name = "paymentReference")]
        public string PaymentReference { get; set; }
    }
}