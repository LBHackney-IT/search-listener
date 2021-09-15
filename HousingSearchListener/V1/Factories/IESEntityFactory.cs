using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;

namespace HousingSearchListener.V1.Factories
{
    public interface IESEntityFactory
    {
        QueryablePerson CreatePerson(Person person);
        QueryableTenure CreateQueryableTenure(TenureInformation tenure);
    }
}