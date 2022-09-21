using System;

namespace HousingSearchListener.V1.Infrastructure.Exceptions
{
    public class EntityNotIndexedException<T> : Exception where T : class
    {
        public string EntityName => typeof(T).Name;
        public string Id { get; }

        public EntityNotIndexedException(string id)
            : base($"{typeof(T).Name} with id {id} not indexed in ElasticSearch.")
        {
            Id = id;
        }
    }
}
