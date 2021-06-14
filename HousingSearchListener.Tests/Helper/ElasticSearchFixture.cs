using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using HousingSearchListener.Infrastructure;
using Nest;

namespace HousingSearchListener.Tests.Helper
{
    public class ElasticSearchFixture : BaseFunction, IDisposable
    {
        public ElasticSearchFixture()
        {
            TestDataHelper.InsertPersonInEs(ServiceProvider.GetService<IElasticClient>());

            // For the index to have time to be populated
            Thread.Sleep(500);
        }

        public void Dispose()
        {

        }
    }
}
