using System.Collections.Generic;

namespace HousingSearchListener.V1.Domain.ElasticSearch
{
    public class QueryableTenure
    {
        public string Id { get; set; }
        public string PaymentReference { get; set; }
        public List<QueryableHouseholdMember> HouseholdMembers { get; set; }
        public QueryableTenuredAsset TenuredAsset { get; set; }
        public string StartOfTenureDate { get; set; }
        public string EndOfTenureDate { get; set; }
        public QueryableTenureType TenureType { get; set; }
    }
}
