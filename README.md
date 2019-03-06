# GraphQL.CacheProvider
GraphQL CacheProvider help storing graphQL queries in cache.

## How to :

### Grab the package
    Install-Package GraphQL.CacheProvider
or

    dotnet add package GraphQL.CacheProvider

---

### Register the cache in startup class

```csharp
public void ConfigureServices(IServiceCollection services)
{
    [...]

    services.AddGraphQLCache();

    [...]
}
```
---

### Inject cache into your service and register actions
```csharp
private readonly IGraphQLCacheProvider<Entity> cache;

public FooService(IGraphQLCacheProvider<Entity> cache)
        {
            this.cache = cache;
            this.cache.RegisterActions(
                getFromDatabaseById: this.GetFromDatabaseById,
                getFromDatabase: this.GetFromDatabase,
                getByIdFromList: this.GetByIdFromList);
        }
```
---

### Actions: 

 - getFromDatabase: Put there your logic to retrieve your entities from the database
   - Arguments : 
     - context: ReloveFieldContext\<object>: The graphQL context
     - items: out List<Entity>: The result that will be set in cache
 - getFromDatabaseById: Put there your logic to retrieve your entity by the specified id from the database
   - Arguments :
     - id: object: the id of the entity you want to retrieve
     - context: ResolveFieldContext\<object>: The graphQL context
     - item: out Entity: The result that will be set in cache
 - getByIdFromList: Logic to get an item out of a list by its id.
   - Arguments :
     - items: List<Entity>: The list where you'll find (or not) the item by its id.
     - id: object: the item's id.
     - item: out Entity: The item result
---
### Main methods

#### Get list of entities

```csharp
List<T> Get(ResolveFieldContext<object> context);
```
> Returns a list of entity from cache if already fetched or from database.

#### Get entity by its id

```csharp
T GetById(object id, ResolveFieldContext<object> context);
```
> Returns a single entity from cache if already fetched or from database.

#### Add entity to cache

```csharp
void Add(T item, IDictionary<string, Field> fields);
```
> When you need to insert an entity, add it also to the cache.


#### Set cache options

```csharp
void SetCacheOptions(MemoryCacheEntryOptions cacheEntryOptions);
```
> Set custom cache entry options. [See more on microsoft docs](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.memorycacheentryoptions?view=aspnetcore-2.2)


