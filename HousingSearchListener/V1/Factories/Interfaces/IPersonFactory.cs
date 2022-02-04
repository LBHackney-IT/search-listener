using Hackney.Shared.HousingSearch.Gateways.Models.Persons;
using HousingSearchListener.V1.Domain.Person;

namespace HousingSearchListener.V1.Factories.Interfaces
{
    public interface IPersonFactory
    {
        QueryablePerson CreatePerson(Person person);
    }
}
