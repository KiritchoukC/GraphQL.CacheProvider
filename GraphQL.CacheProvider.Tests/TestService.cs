namespace GraphQL.CacheProvider.Tests
{
    using global::GraphQL.CacheProvider.Interfaces;
    using GraphQL.Language.AST;
    using GraphQL.Types;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class TestService : IClassFixture<TestFixture>
    {
        #region Private Fields

        private readonly TestFixture fixture;
        private int databaseRetrieves = 0;
        private IGraphQLCacheProvider<TestEntity> memoryCacheProvider;

        #endregion Private Fields

        #region Public Constructors

        public TestService(TestFixture fixture)
        {
            this.fixture = fixture;
            this.memoryCacheProvider = fixture.MemoryCacheProvider;
            this.memoryCacheProvider.RegisterActions(
                getByIdFromList: GetByIdFromList,
                getFromDatabase: GetFromDatabase,
                getFromDatabaseById: GetFromDatabaseById);
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Make a first call with all the fields and then another one with less fields.
        /// The first one will retrieve entities from database and store it in cache.
        /// The second one with less fields will retrieve its entities from the other cache.
        /// </summary>
        [Fact]
        public void GetFromAnotherCache()
        {
            databaseRetrieves = 0;

            ResolveFieldContext<object> context = new ResolveFieldContext<object>();
            context.SubFields = new Dictionary<string, Field>();
            context.SubFields.Add("id", new Field());
            context.SubFields.Add("name", new Field());
            context.SubFields.Add("description", new Field());
            context.SubFields.Add("creationDate", new Field());
            context.SubFields.Add("creationUser", new Field());

            // Make a first call with all the fields
            var actualFull = this.memoryCacheProvider.Get(context);
            var expectedFull = TestEntity.Get();

            Assert.True(actualFull.Count == expectedFull.Count);
            Assert.True(databaseRetrieves == 1);

            for (int i = 0; i < actualFull.Count; i++)
            {
                Assert.True(actualFull[i].Equals(expectedFull[i]));
            }

            // Make a second call with less fields. Resources must be retrieved from the other cache.
            context.SubFields.Remove("creationDate");
            context.SubFields.Remove("creationUser");

            var actual = this.memoryCacheProvider.Get(context);
            var expected = TestEntity.Get();

            Assert.True(actual.Count == expected.Count);
            Assert.True(databaseRetrieves == 1);

            for (int i = 0; i < actual.Count; i++)
            {
                Assert.True(actual[i].Equals(expected[i]));
            }
        }

        /// <summary>
        /// Get the entities from database first and the second call should get the entities from the cache.
        /// </summary>
        [Fact]
        public void GetFromCache()
        {
            databaseRetrieves = 0;

            ResolveFieldContext<object> context = new ResolveFieldContext<object>();
            context.SubFields = new Dictionary<string, Field>();
            context.SubFields.Add("id", new Field());
            context.SubFields.Add("name", new Field());
            context.SubFields.Add("description", new Field());

            // First get it from db
            var actualDb = this.memoryCacheProvider.Get(context);
            var expectedDb = TestEntity.Get();

            Assert.True(actualDb.Count == expectedDb.Count);
            Assert.True(databaseRetrieves == 1);

            for (int i = 0; i < actualDb.Count; i++)
            {
                Assert.True(actualDb[i].Equals(expectedDb[i]));
            }

            // Second get it from cache.
            var actual = this.memoryCacheProvider.Get(context);
            var expected = TestEntity.Get();

            Assert.True(actual.Count == expected.Count);
            Assert.True(databaseRetrieves == 1);

            for (int i = 0; i < actual.Count; i++)
            {
                Assert.True(actual[i].Equals(expected[i]));
            }
        }

        /// <summary>
        /// Get the items from database
        /// </summary>
        [Fact]
        public void GetFromDb()
        {
            databaseRetrieves = 0;

            ResolveFieldContext<object> context = new ResolveFieldContext<object>();
            context.SubFields = new Dictionary<string, Field>();
            context.SubFields.Add("id", new Field());
            context.SubFields.Add("name", new Field());

            var actual = this.memoryCacheProvider.Get(context);
            var expected = TestEntity.Get();

            Assert.True(actual.Count == expected.Count);
            Assert.True(databaseRetrieves == 1);

            for (int i = 0; i < actual.Count; i++)
            {
                Assert.True(actual[i].Equals(expected[i]));
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Get an item by its id from a given list.
        /// </summary>
        /// <param name="items"> </param>
        /// <param name="id">    </param>
        /// <param name="result"></param>
        private void GetByIdFromList(List<TestEntity> items, object id, out TestEntity result) => result = items.FirstOrDefault(x => x.Id == (int)id);

        /// <summary>
        /// Build the query and execute it on database.
        /// </summary>
        /// <param name="context">The GraphQL Context</param>
        /// <returns></returns>
        private void GetFromDatabase(ResolveFieldContext<object> context, out List<TestEntity> items)
        {
            items = TestEntity.Get();
            this.databaseRetrieves++;
        }

        /// <summary>
        /// Build the query and execute it on database.
        /// </summary>
        /// <param name="context">The GraphQL Context</param>
        /// <param name="id">     The entity identifier</param>
        /// <returns></returns>
        private void GetFromDatabaseById(object id, ResolveFieldContext<object> context, out TestEntity item)
        {
            item = TestEntity.Get().FirstOrDefault();
            this.databaseRetrieves++;
        }

        #endregion Private Methods
    }
}