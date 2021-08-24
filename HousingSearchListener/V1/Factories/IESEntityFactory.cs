using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;

namespace HousingSearchListener.V1.Factories
{
    public interface IESEntityFactory
    {
        ESPerson CreatePerson(Person person);
        QueryableTenure CreateQueryableTenure(TenureInformation tenure);
    }
}