using System.Runtime.InteropServices.ComTypes;
using HousingSearchListener.Tests.Stubs;
using HousingSearchListener.V1.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HousingSearchListener.Tests.E2E
{
    [Collection("ElasticSearch collection")]
    public class IntegrationTests
    {
        private ElasticSearchUpdater _sut;
        public IntegrationTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IHttpHandler, HttpHandlerStub>();

            _sut = new ElasticSearchUpdater(serviceCollection);
        }
    }
}
