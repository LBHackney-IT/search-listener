using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using HousingSearchListener.Gateways;
using HousingSearchListener.Infrastructure;
using HousingSearchListener.V1.Domain;
using HousingSearchListener.V1.Gateway;
using HousingSearchListener.V1.UseCase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace HousingSearchListener.V1.Interfaces
{
    public class ElasticSearchService : BaseService, IElasticSearchService
    {
        private IESPersonFactory _esPersonFactory;
        private IHttpHandler _httpHandler;
        private IPersonMessageFactory _personMessageFactory;
        private ICreatePersonUseCase _createPersonUseCase;
        private IUpdatePersonUseCase _updatePersonUseCase;

        public ElasticSearchService(IServiceCollection services) : base(services)
        {
            _esPersonFactory = ServiceProvider.GetService<IESPersonFactory>();
            _httpHandler = ServiceProvider.GetService<IHttpHandler>();
            _personMessageFactory = ServiceProvider.GetService<IPersonMessageFactory>();
            _createPersonUseCase = ServiceProvider.GetService <ICreatePersonUseCase>();
            _updatePersonUseCase = ServiceProvider.GetService <IUpdatePersonUseCase>();
        }

        public async Task Process(SQSEvent sqsEvent)
        {
            foreach (var record in sqsEvent.Records)
            {
                var result = await GetPersonFromPersonApi(record);
                await Process(record, result);
            }
        }

        private async Task Process(SQSEvent.SQSMessage record, HttpResponseMessage result)
        {
            var personCreatedMessage = _personMessageFactory.Create(record);
            var personString = await result.Content.ReadAsStringAsync();
            var person = JsonConvert.DeserializeObject<Person>(personString);
            var esPerson = _esPersonFactory.Create(person);

            switch (personCreatedMessage.EventType)
            {
                case EventTypes.PersonCreatedEvent:
                    await _createPersonUseCase.Create(esPerson);
                    break;
                case EventTypes.PersonUpdatedEvent:
                    await _updatePersonUseCase.Update(esPerson);
                    break;
                default:
                    throw new NotImplementedException(
                        $"ES updated for eventtype {personCreatedMessage.EventType} not implemented");
            }
        }

        private async Task<HttpResponseMessage> GetPersonFromPersonApi(SQSEvent.SQSMessage record)
        {
            var personCreatedMessage = _personMessageFactory.Create(record);
            var personApiUrl = $"{Environment.GetEnvironmentVariable("PersonApiUrl")}/persons/{personCreatedMessage.EntityId}";

            _httpHandler.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(Environment.GetEnvironmentVariable("PersonApiToken"));

            var result = await _httpHandler.GetAsync(personApiUrl);

            if (!result.IsSuccessStatusCode)
                throw new Exception(result.Content.ReadAsStringAsync().Result);

            return result;
        }

        #region Register Dependencies

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            RegisterDependencies(services);
        }

        private void RegisterDependencies(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpHandler, HttpClientHandler>();

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddScoped<IPersonMessageFactory, PersonMessageFactory>();
            services.AddScoped<IESPersonFactory, EsPersonFactory>();
            services.AddScoped<ICreatePersonUseCase, CreatePersonUseCase>();
            services.AddScoped<IUpdatePersonUseCase, UpdatePersonUseCase>();
            services.AddScoped<IEsGateway, EsGateway>();

            ESServiceInitializer.Initialize(services);
        }

        #endregion Register Dependencies
    }
}