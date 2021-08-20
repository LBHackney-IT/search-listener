using System;

namespace HousingSearchListener.V1.Infrastructure.Exceptions
{
    public class PersonNotFoundException : EntityNotFoundException
    {
        public PersonNotFoundException(Guid id)
            : base("Person", id)
        { }
    }
}
