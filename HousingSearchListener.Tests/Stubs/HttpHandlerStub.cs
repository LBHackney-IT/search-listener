using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using AutoFixture;
using HousingSearchListener.V1.Domain;
using HousingSearchListener.V1.Interfaces;
using Newtonsoft.Json;

namespace HousingSearchListener.Tests.Stubs
{
    public class HttpHandlerStub : IHttpHandler
    {
        private static Person _person;

        static HttpHandlerStub()
        {
            _person = new Fixture().Build<Person>().Create();
        }

        public HttpResponseMessage Get(string url)
        {
            throw new NotImplementedException();
        }

        public HttpResponseMessage Post(string url, HttpContent content)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(_person))
            };
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(_person))
            };
        }

        public HttpRequestHeaders DefaultRequestHeaders => new HttpRequestMessage().Headers;
    }
}
