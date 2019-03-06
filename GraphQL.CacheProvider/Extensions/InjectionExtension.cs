
namespace GraphQL.CacheProvider.Extensions
{
    using GraphQL.CacheProvider.Cache;
    using GraphQL.CacheProvider.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    public static class InjectionExtension
    {
        public static void AddGraphQLCache(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IGraphQLCacheProvider<>), typeof(GraphQLCacheProvider<>));
        }
    }
}
