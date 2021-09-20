using HousingSearchListener.V1.Domain.Person;

namespace HousingSearchListener.V1.UseCase.Interfaces
{
    public interface IIndexCreatePersonUseCase : IMessageProcessing
    {
        public Person Person { get; set; }
    }
}
