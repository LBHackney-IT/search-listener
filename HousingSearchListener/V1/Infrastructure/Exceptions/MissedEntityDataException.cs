using System;

namespace HousingSearchListener.V1.Infrastructure.Exceptions
{
    public class MissedEntityDataException : ArgumentException
    {
        public MissedEntityDataException(string message)
            : base(message) { }
    }
}
