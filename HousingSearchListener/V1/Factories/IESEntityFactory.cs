using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using System.Collections.Generic;

namespace HousingSearchListener.V1.Factories
{
    public interface IESEntityFactory
    {
        ESPerson CreatePerson(Person person);
        ESPersonTenure CreateTenure(TenureInformation tenure);
        QueryableTenure CreateQueryableTenure(TenureInformation tenure);
        List<QueryableHouseholdMember> CreateQueryableHouseholdMembers(List<HouseholdMembers> householdMembers);
        Domain.ElasticSearch.Asset.QueryableTenure CreateAssetQueryableTenure(TenureInformation tenure);
    }
}