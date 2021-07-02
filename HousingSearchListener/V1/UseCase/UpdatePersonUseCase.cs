using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using HousingSearchListener.Gateways;
using HousingSearchListener.V1.Domain;
using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Gateway;
using Newtonsoft.Json;

namespace HousingSearchListener.V1.UseCase
{
    public class UpdatePersonUseCase : IUpdatePersonUseCase
    {
        private readonly IEsGateway _esGateway;

        public UpdatePersonUseCase(IEsGateway esGateway)
        {
            _esGateway = esGateway;
        }

        public async Task Update(ESPerson esPerson)
        {
            await _esGateway.Update(esPerson);
        }
    }
}