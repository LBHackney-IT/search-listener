﻿using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using HousingSearchListener.Infrastructure;
using Nest;

namespace HousingSearchListener.Tests.Helper
{
    public class ElasticSearchFixture : IDisposable
    {
        public ElasticSearchFixture()
        {
            var serviceCollection = new ServiceCollection();
            ESServiceInitializer.Initialize(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            TestDataHelper.CreateEsIndex(serviceProvider.GetService<IElasticClient>());

            // For the index to have time to be populated
            Thread.Sleep(500);
        }

        public void Dispose()
        {

        }
    }
}
