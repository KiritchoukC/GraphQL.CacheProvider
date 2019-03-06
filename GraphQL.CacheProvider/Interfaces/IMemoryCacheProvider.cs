namespace GraphQL.CacheProvider.Interfaces
{
    using GraphQL.CacheProvider.Delegates;
    using GraphQL.Language.AST;
    using GraphQL.Types;
    using Microsoft.Extensions.Caching.Memory;
    using System.Collections.Generic;

    public interface IMemoryCacheProvider<T> : IMemoryCache
    {
        #region Public Methods

        void Add(T item, IDictionary<string, Field> fields);

        List<T> Get(ResolveFieldContext<object> context);

        T GetById(object id, ResolveFieldContext<object> context);

        void RegisterActions(
            OutAction<object, ResolveFieldContext<object>, T> getFromDatabaseById,
            OutAction<ResolveFieldContext<object>, List<T>> getFromDatabase,
            OutAction<List<T>, object, T> getByIdFromList);

        void SetCacheOptions(MemoryCacheEntryOptions cacheEntryOptions);

        #endregion Public Methods
    }
}