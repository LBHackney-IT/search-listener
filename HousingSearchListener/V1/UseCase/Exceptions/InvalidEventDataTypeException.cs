using System;

namespace HousingSearchListener.V1.UseCase.Exceptions
{
    public class InvalidEventDataTypeException<T> : System.Exception where T : class
    {
        public string EntityName => typeof(T).Name;
        public Guid Id { get; }

        public InvalidEventDataTypeException(Guid id)
            : base($"Expected EventData of message with id {id} to be of type {typeof(T).Name}.")
        {
            Id = id;
        }
    }
}