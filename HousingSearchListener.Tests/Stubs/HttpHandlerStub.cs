using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HousingSearchListener.V1.Domain;
using HousingSearchListener.V1.Interfaces;
using Newtonsoft.Json;

namespace HousingSearchListener.Tests.Stubs
{
    public class HttpHandlerStub : IHttpHandler
    {
        public HttpResponseMessage Get(string url)
        {
            throw new NotImplementedException();
        }

        public HttpResponseMessage Post(string url, HttpContent content)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new AutoFixture.Fixture().Build<Person>()))
            };
        }
    }
}
