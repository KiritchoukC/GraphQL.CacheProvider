namespace GraphQL.CacheProvider.Cache
{
    using GraphQL.CacheProvider.Delegates;
    using GraphQL.CacheProvider.Interfaces;
    using GraphQL.Language.AST;
    using GraphQL.Types;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GraphQLCacheProvider<T> : MemoryCache, IGraphQLCacheProvider<T>
    {
        #region Private Fields

        private static IOptions<MemoryCacheOptions> options = new MemoryCacheOptions();
        private MemoryCacheEntryOptions cacheEntryOptions;
        private string CacheKeysCacheKey = $"{typeof(T).Name}_CacheKeysCacheKey";
        private OutAction<List<T>, object, T> GetByIdFromList;
        private OutAction<ResolveFieldContext<object>, List<T>> GetFromDatabase;
        private OutAction<object, ResolveFieldContext<object>, T> GetFromDatabaseById;

        #endregion Private Fields

        #region Public Constructors

        public GraphQLCacheProvider() : base(options)
        {
            cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddDays(1)
            };
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Add the specified item to cache.
        /// </summary>
        /// <param name="item">   Item to add to the cache.</param>
        /// <param name="context">The GraphQL context.</param>
        public void Add(T item, IDictionary<string, Field> fields)
        {
            try
            {
                string cacheKey = this.GetCacheKey(fields);

                if (this.TryGetValue(cacheKey, out List<T> items))
                {
                    // Add the item to the cached list
                    items.Add(item);

                    // Remove the cache and recreate it with the updated list.
                    this.Remove(cacheKey);
                    this.Set(cacheKey, items, cacheEntryOptions);
                }
                else
                {
                    // If cache does not exist yet. Create it with a list of a single item.
                    this.Set(cacheKey, new List<T> { item }, cacheEntryOptions);
                    // Add cache key to cache
                    AddCacheKey(cacheKey);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get a list of entities.
        /// </summary>
        /// <param name="context">The GraphQL Context</param>
        /// <returns></returns>
        public List<T> Get(ResolveFieldContext<object> context)
        {
            List<T> result = null;

            try
            {
                string cacheKey = GetCacheKey(context.SubFields);
                if (!this.TryGetValue(cacheKey, out result))
                {
                    // Check if another cache with the same fields already exists...
                    string sameCacheKey = this.GetAlreadyExistingCacheKey(cacheKey);

                    if (!string.IsNullOrEmpty(sameCacheKey))
                    {
                        // If it does get its cached list.
                        result = this.Get<List<T>>(sameCacheKey);
                    }
                    else
                    {
                        // If it does not, get the result from database.
                        this.GetFromDatabase(context, out result);

                        // And add it to cache.
                        this.Add(result, cacheKey);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// Get the entity by its id.
        /// </summary>
        /// <param name="id">     </param>
        /// <param name="context"></param>
        /// <returns></returns>
        public T GetById(object id,
            ResolveFieldContext<object> context)
        {
            T result = default;

            try
            {
                string cacheKey = GetCacheKey(context.SubFields);
                if (this.TryGetValue(cacheKey, out List<T> items))
                {
                    // If cache exists, get the required item by the specified id.
                    GetByIdFromList(items, id, out result);
                }
                else
                {
                    // Check if another cache with the same fields already exists...
                    string sameCacheKey = this.GetAlreadyExistingCacheKey(cacheKey);

                    if (!string.IsNullOrEmpty(sameCacheKey))
                    {
                        // If it does exist, get its cached list and get only the required item.
                        items = this.Get<List<T>>(sameCacheKey);
                        GetByIdFromList(items, id, out result);
                    }
                    else
                    {
                        // If it does not get the item from database by the specified id.
                        GetFromDatabaseById(id, context, out result);

                        // And add it to the cache.
                        this.Add(result, id, cacheKey);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// Registers all needed actions
        /// </summary>
        /// <param name="getFromDatabaseById"></param>
        /// <param name="getFromDatabase">    </param>
        /// <param name="getByIdFromList">    </param>
        public void RegisterActions(
            OutAction<object, ResolveFieldContext<object>, T> getFromDatabaseById,
            OutAction<ResolveFieldContext<object>, List<T>> getFromDatabase,
            OutAction<List<T>, object, T> getByIdFromList)
        {
            this.GetFromDatabaseById = getFromDatabaseById;
            this.GetFromDatabase = getFromDatabase;
            this.GetByIdFromList = getByIdFromList;
        }

        /// <summary>
        /// Set the cache options
        /// </summary>
        /// <param name="cacheEntryOptions">The cache entry options</param>
        public void SetCacheOptions(MemoryCacheEntryOptions cacheEntryOptions)
        {
            this.cacheEntryOptions = cacheEntryOptions;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Add a list of entities to the cache
        /// </summary>
        /// <param name="items">   </param>
        /// <param name="cacheKey"></param>
        private void Add(List<T> items, string cacheKey)
        {
            try
            {
                if (this.TryGetValue(cacheKey, out List<T> cache))
                {
                    // If the cache already exists, remove it.
                    this.Remove(cacheKey);
                }
                // Set the cache.
                this.Set(cacheKey, items, cacheEntryOptions);
                // And add the cache key to the cache.
                AddCacheKey(cacheKey);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Add the provided entity to the cache.
        /// </summary>
        /// <param name="item">    </param>
        /// <param name="cacheKey"></param>
        private void Add(T item, object id, string cacheKey)
        {
            try
            {
                List<T> items = new List<T>();

                if (this.TryGetValue(cacheKey, out items))
                {
                    this.GetByIdFromList(items, id, out T cachedItem);

                    if (cachedItem != null) items.Remove(cachedItem);

                    items.Add(item);
                    this.Remove(cacheKey);
                    this.Set(cacheKey, items, cacheEntryOptions);
                }
                else
                {
                    this.Set(cacheKey, new List<T> { item }, cacheEntryOptions);
                    // Add cache key to cache
                    AddCacheKey(cacheKey);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Add the cache key to its own cache.
        /// </summary>
        /// <param name="cacheKey"></param>
        private void AddCacheKey(string cacheKey)
        {
            try
            {
                if (this.TryGetValue(CacheKeysCacheKey, out List<string> keys))
                {
                    keys.Add(cacheKey);
                    this.Remove(CacheKeysCacheKey);
                    this.Set(CacheKeysCacheKey, keys, cacheEntryOptions);
                }
                else
                {
                    this.Set(CacheKeysCacheKey, new List<string>() { cacheKey }, cacheEntryOptions);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// If all fields ar contained in another cache then return this cache key.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns>The cache key</returns>
        private string GetAlreadyExistingCacheKey(string cacheKey)
        {
            string result = string.Empty;

            try
            {
                if (this.TryGetValue(CacheKeysCacheKey, out List<string> cacheKeys))
                {
                    var fields = GetFieldsFromCacheKey(cacheKey);
                    int cacheKeyCount = 0;

                    foreach (var key in cacheKeys)
                    {
                        var keyFields = this.GetFieldsFromCacheKey(key);
                        if (fields.Count < keyFields.Count)
                        {
                            bool sameCache = false;
                            foreach (var field in fields)
                            {
                                sameCache = keyFields.Any(x => x == field);
                                if (!sameCache) break;
                            }
                            if (sameCache && keyFields.Count > cacheKeyCount)
                            {
                                result = key;
                                cacheKeyCount = keyFields.Count;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// Format and return the cache key.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetCacheKey(IDictionary<string, Field> fields) => $"{typeof(T).Name}_{string.Join("_", fields.Select(x => x.Key).ToArray())}";

        /// <summary>
        /// Split the cache key and return a list of fields.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        private List<string> GetFieldsFromCacheKey(string cacheKey)
        {
            List<string> result = new List<string>();

            try
            {
                var split = cacheKey.Split('_');
                for (int i = 1; i < split.Count(); i++)
                {
                    result.Add(split[i]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        #endregion Private Methods
    }
}