using HousingSearchListener.V1.Domain;
using HousingSearchListener.V1.Domain.ElasticSearch;

namespace HousingSearchListener.Gateways
{
    public interface IESPersonFactory
    {
        ESPerson Create(Person person);
    }
}