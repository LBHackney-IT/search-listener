//using Amazon.Lambda.SQSEvents;
//using HousingSearchListener.V1.Domain;
//using HousingSearchListener.V1.Factories;
//using HousingSearchListener.V1.Gateway;
//using HousingSearchListener.V1.Infrastructure;
//using HousingSearchListener.V1.UseCase;
//using HousingSearchListener.V1.UseCase.Interfaces;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection.Extensions;
//using Newtonsoft.Json;
//using System;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Threading.Tasks;

//namespace HousingSearchListener.V1.Interfaces
//{
//    public class ElasticSearchService : IElasticSearchService
//    {
//        private IESEntityFactory _esPersonFactory;
//        private IHttpHandler _httpHandler;
//        private IMessageFactory _messageFactory;
//        private IIndexPersonUseCase _createPersonUseCase;
//        private IUpdatePersonUseCase _updatePersonUseCase;

//        public ElasticSearchService(IServiceCollection services) : base(services)
//        {
//            _esPersonFactory = ServiceProvider.GetService<IESEntityFactory>();
//            _httpHandler = ServiceProvider.GetService<IHttpHandler>();
//            _messageFactory = ServiceProvider.GetService<IMessageFactory>();

//            _createPersonUseCase = ServiceProvider.GetService<IIndexPersonUseCase>();
//            _updatePersonUseCase = ServiceProvider.GetService<IUpdatePersonUseCase>();
//        }

//        public async Task Process(EntityEventSns sqsEvent)
//        {
//            foreach (var record in sqsEvent.Records)
//            {
//                var result = await GetPersonFromPersonApi(record);
//                await Process(record, result);
//            }
//        }

//        private async Task Process(SQSEvent.SQSMessage record, HttpResponseMessage result)
//        {
//            var message = _messageFactory.Create(record);
//            var personString = await result.Content.ReadAsStringAsync();
//            var person = JsonConvert.DeserializeObject<Person>(personString);
//            var esPerson = _esPersonFactory.Create(person);

//            switch (message.EventType)
//            {
//                case EventTypes.PersonCreatedEvent:
//                    await _createPersonUseCase.Create(esPerson);
//                    break;
//                case EventTypes.PersonUpdatedEvent:
//                    await _updatePersonUseCase.Update(esPerson);
//                    break;
//                case EventTypes.TenureCreatedEvent:
//                    await _createPersonUseCase.Update(esPerson);
//                    break;
//                default:
//                    throw new NotImplementedException(
//                        $"ES updated for eventtype {message.EventType} not implemented");
//            }
//        }

//        private async Task<HttpResponseMessage> GetPersonFromPersonApi(SQSEvent.SQSMessage record)
//        {
//            var personCreatedMessage = _messageFactory.Create(record);
//            var personApiUrl = $"{Environment.GetEnvironmentVariable("PersonApiUrl")}/persons/{personCreatedMessage.EntityId}";

//            _httpHandler.DefaultRequestHeaders.Authorization =
//                new AuthenticationHeaderValue(Environment.GetEnvironmentVariable("PersonApiToken"));

//            var result = await _httpHandler.GetAsync(personApiUrl);

//            if (!result.IsSuccessStatusCode)
//                throw new Exception(result.Content.ReadAsStringAsync().Result);

//            return result;
//        }

//        #region Register Dependencies

//        protected override void ConfigureServices(IServiceCollection services)
//        {
//            base.ConfigureServices(services);
//            RegisterDependencies(services);
//        }

//        private void RegisterDependencies(IServiceCollection services)
//        {
//            services.TryAddSingleton<IHttpHandler, HttpClientHandler>();

//            services.AddSingleton<IConfiguration>(Configuration);
//            services.AddScoped<IESEntityFactory, EsEntityFactory>();
//            services.AddScoped<IIndexPersonUseCase, IndexPersonUseCase>();
//            services.AddScoped<IUpdatePersonUseCase, UpdatePersonUseCase>();
//            services.AddScoped<IEsGateway, EsGateway>();

//            ESServiceInitializer.ConfigureElasticSearch(services);
//        }

//        #endregion Register Dependencies
//    }
//}