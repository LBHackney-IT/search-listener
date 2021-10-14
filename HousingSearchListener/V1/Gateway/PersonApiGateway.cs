using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.Person;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class PersonApiGateway : IPersonApiGateway
    {
        private const string ApiName = "Person";
        private const string PersonApiUrl = "PersonApiUrl";
        private const string PersonApiToken = "PersonApiToken";

        private readonly IApiGateway _apiGateway;

        public PersonApiGateway(IApiGateway apiGateway)
        {
            _apiGateway = apiGateway;
            _apiGateway.Initialise(ApiName, PersonApiUrl, PersonApiToken);
        }

        [LogCall]
        public async Task<Person> GetPersonByIdAsync(Guid id, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/persons/{id}";
            return await _apiGateway.GetByIdAsync<Person>(route, id, correlationId);
        }
    }
}
