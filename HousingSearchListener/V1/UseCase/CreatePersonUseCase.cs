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
    public class CreatePersonUseCase : ICreatePersonUseCase
    {
        private readonly IEsGateway _esGateway;

        public CreatePersonUseCase(IEsGateway esGateway)
        {
            _esGateway = esGateway;
        }

        public async Task Create(ESPerson esPerson)
        {
            await _esGateway.Create(esPerson);
        }
    }
}