using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using Hackney.Shared.HousingSearch.Gateways.Models.Tenures;
using HousingSearchListener.V1.Domain.Tenure;
using System.Collections.Generic;
using Hackney.Shared.HousingSearch.Gateways.Models.Transactions;
using HousingSearchListener.V1.Domain.Transaction;
using Person = HousingSearchListener.V1.Domain.Person.Person;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Gateways.Models.Processes;
using Hackney.Shared.HousingSearch.Domain.Process;

namespace HousingSearchListener.V1.Factories
{
    public interface IESEntityFactory
    {
        QueryablePerson CreatePerson(Person person);
        QueryableTenure CreateQueryableTenure(TenureInformation tenure);
        List<QueryableHouseholdMember> CreateQueryableHouseholdMembers(List<HouseholdMembers> householdMembers);
        QueryableAssetTenure CreateAssetQueryableTenure(TenureInformation tenure);
        QueryableTransaction CreateQueryableTransaction(TransactionResponseObject transaction);
        QueryableAsset CreateAsset(Hackney.Shared.HousingSearch.Domain.Asset.Asset asset);
    }
}