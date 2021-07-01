using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HousingSearchListener.V1.Domain.ElasticSearch;
using Nest;

namespace HousingSearchListener.V1.Gateway
{
    public interface IEsGateway
    {
        Task<IndexResponse> Update(ESPerson esPerson);

        Task<IndexResponse> Create(ESPerson esPerson);
    }
}
