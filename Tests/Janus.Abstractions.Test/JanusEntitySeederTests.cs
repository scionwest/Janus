using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus.Seeding
{
    [TestClass]
    public class JanusEntitySeederTests
    {
        [TestMethod]
        public void SeedType_IsSetFromGenericType()
        {
            // Arrange
            var seeder = Mock.Of<JanusEntitySeeder<TestEntity>>();

            // Act
            Type entityType = seeder.SeedType;

            // Act
            Assert.AreEqual(typeof(TestEntity), entityType);
        }

        [TestMethod]
        public void SeedData_IsSetWhenGenerated()
        {
            // Arrange
            var fakeData = new List<TestEntity> { new TestEntity() };
            var mock = new Mock<JanusEntitySeeder<TestEntity>>();
            mock.Protected()
                .Setup<IList<TestEntity>>("Seed", ItExpr.IsAny<JanusSeedOptions>())
                .Returns(fakeData);

            IEntitySeeder<TestEntity> seeder = mock.Object;

            // Act
            seeder.Generate();
            TestEntity[] entities = seeder.GetSeedEntities();

            // Assert
            Assert.AreEqual(fakeData.Count, entities.Length);
            Assert.AreEqual(fakeData[0], entities[0]);
        }

        [TestMethod]
        public void BuildRelationships_MapsDataTogether()
        {
            // Arrange
            var mock = new Mock<JanusEntitySeeder<TestEntity>>();
            mock.Protected()
                .Setup<bool>("MapEntities", ItExpr.IsAny<TestEntity[]>(), ItExpr.IsAny<ISeedReader>())
                .Returns(true);
            IEntitySeeder<TestEntity> seeder = mock.Object;

            // Act
            bool isMapped = seeder.BuildRelationships(Mock.Of<ISeedReader>());

            // Assert
            Assert.IsTrue(isMapped);
            mock.Protected().Verify("MapEntities", Times.Once(), ItExpr.IsAny<TestEntity[]>(), ItExpr.IsAny<ISeedReader>());
        }

        [TestMethod]
        public void ValidateSeedData_RemovesBadData()
        {
            // Arrange
            var data = new List<TestEntity>() { new TestEntity { Value = "Hello World" }, new TestEntity { Value = null } };
            var mock = new Mock<JanusEntitySeeder<TestEntity>>();
            //Func<TestEntity, IDatabaseSeedReader, true> validator = (entity,
            mock.Protected()
                .Setup<IList<TestEntity>>("Seed", ItExpr.IsAny<JanusSeedOptions>())
                .Returns(data.ToList());
            mock.Protected()
                .Setup<bool>("IsEntityValid", ItExpr.IsAny<TestEntity>(), ItExpr.IsAny<ISeedReader>())
                .Returns<TestEntity, ISeedReader>((entity, reader) => entity.Value != null);

            IEntitySeeder<TestEntity> seeder = mock.Object;

            // Act
            seeder.Generate();
            seeder.ValidateSeedData(Mock.Of<ISeedReader>());
            TestEntity[] validatedData = seeder.GetSeedEntities();

            // Assert
            mock.Protected().Verify("IsEntityValid", Times.Exactly(data.Count), ItExpr.IsAny<TestEntity>(), ItExpr.IsAny<ISeedReader>());
            Assert.AreEqual(1, validatedData.Length);
            Assert.AreEqual(data[0], validatedData[0]);
        }
    }
}
