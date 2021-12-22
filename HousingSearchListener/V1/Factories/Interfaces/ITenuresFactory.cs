using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Tenure;
using System.Collections.Generic;

namespace HousingSearchListener.V1.Factories.Interfaces
{
    public interface ITenuresFactory
    {
        QueryableAssetTenure CreateAssetQueryableTenure(TenureInformation tenure);
        QueryableTenure CreateQueryableTenure(TenureInformation tenure);
        List<QueryableHouseholdMember> CreateQueryableHouseholdMembers(List<HouseholdMembers> householdMembers);

    }
}
