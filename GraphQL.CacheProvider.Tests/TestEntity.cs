namespace GraphQL.CacheProvider.Tests
{
    using System;
    using System.Collections.Generic;

    public class TestEntity
    {
        #region Public Properties

        public DateTime CreationDate { get; set; }
        public string CreationUser { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        #endregion Public Properties

        public static List<TestEntity> Get()
        {
            return new List<TestEntity>
            {
                new TestEntity
                {
                    CreationDate = new DateTime(2019, 03, 06, 16, 58, 59),
                    CreationUser = "SYSTEM",
                    Description = "Test entity 1",
                    Name = "Test 1",
                    Id = 1
                },

                new TestEntity
                {
                    CreationDate = new DateTime(2019, 03, 06, 16, 58, 59),
                    CreationUser = "SYSTEM",
                    Description = "Test entity 2",
                    Name = "Test 2",
                    Id = 2
                },

                new TestEntity
                {
                    CreationDate = new DateTime(2019, 03, 06, 16, 58, 59),
                    CreationUser = "SYSTEM",
                    Description = "Test entity 3",
                    Name = "Test 3",
                    Id = 3
                }
            };
        }

        public override bool Equals(object obj)
        {
            return obj is TestEntity entity &&
                   CreationDate == entity.CreationDate &&
                   CreationUser == entity.CreationUser &&
                   Description == entity.Description &&
                   Id == entity.Id &&
                   Name == entity.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CreationDate, CreationUser, Description, Id, Name);
        }
    }
}