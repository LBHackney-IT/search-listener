using System;

namespace HousingSearchListener.V1.Infrastructure.Exceptions
{
    public class AssetNotIndexedException : Exception
    {
        public string Id { get; }

        public AssetNotIndexedException(string id)
            : base($"Asset with id {id} is not indexed in elastic search")
        {
            Id = id;
        }
    }
}
