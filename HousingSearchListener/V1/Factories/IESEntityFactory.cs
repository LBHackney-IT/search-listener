using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using System.Collections.Generic;

namespace HousingSearchListener.V1.Factories
{
    public interface IESEntityFactory
    {
        QueryablePerson CreatePerson(Person person);
        QueryableTenure CreateQueryableTenure(TenureInformation tenure);
        QueryablePersonTenure CreateQueryablePersonTenure(TenureInformation tenure);
        List<QueryableHouseholdMember> CreateQueryableHouseholdMembers(List<HouseholdMembers> householdMembers);
        Domain.ElasticSearch.Asset.QueryableTenure CreateAssetQueryableTenure(TenureInformation tenure);
    }
}