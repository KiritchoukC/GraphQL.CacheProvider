using GraphQL.CacheProvider.Cache;
using GraphQL.CacheProvider.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQL.CacheProvider.Tests
{
    public class TestFixture
    {


        private readonly string DatabaseName;
        public IServiceProvider ServiceProvider { get; set; }
        private bool IsDisposing { get; set; } = false;

        public IGraphQLCacheProvider<TestEntity> MemoryCacheProvider { get; set; }

        public TestFixture()
        {
            MemoryCacheProvider = new GraphQLCacheProvider<TestEntity>();
            var serviceCollection = new ServiceCollection();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
