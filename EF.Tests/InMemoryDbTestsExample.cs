using System;
using System.Data.Common;
using System.Linq;
using EF.Tests.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EF.Tests
{
    public class InMemoryDbTestsExample
    {
        private DbContextOptions<TestContext> _options;
        private DbConnection _connection;

        [OneTimeSetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _options = new DbContextOptionsBuilder<TestContext>()
                .UseSqlite(_connection)
                .Options;
            
            _connection.Open();
            
            using (var context = new TestContext(_options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var one = new Item
                {
                    Id = 1,
                    Name = "ItemOne",
                    Amount = 12
                };
                var two = new Item
                {
                    Id = 2,
                    Name = "ItemTwo",
                    Amount = 24
                };
                var three = new Item
                {
                    Id = 3,
                    Name = "ItemThree",
                    Amount = 33
                };
                
                context.Items.AddRange(one, two, three);
                context.SaveChanges();
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _connection?.Dispose();
        }

        [Test]
        public void GetItems()
        {
            using (var context = new TestContext(_options))
            {
                var items = context.Items.ToArray();

                Assert.AreEqual(3, items.Length);
                Assert.AreEqual("ItemOne", items[0].Name);
                Assert.AreEqual("ItemTwo", items[1].Name);
                Assert.AreEqual("ItemThree", items[2].Name);
            }
        }

        [Test]
        public void UpdateNotTracking()
        {
            using (var context = new TestContext(_options))
            {
                var item = context.Items.AsNoTracking().FirstOrDefault(x => x.Name == "ItemOne");

                item.Name = "NewName";
                context.Items.Update(item);

                context.SaveChanges();
            }
        }

        [Test]
        public void UpdateAfterDelete()
        {
            Action removeFunc = () =>
            {
                using (var context = new TestContext(_options))
                {
                    var item = context.Items.AsNoTracking()
                        .FirstOrDefault(x => x.Name == "ItemOne");

                    context.Items.Remove(item);

                    context.SaveChanges();
                }
            };
            
            using (var context = new TestContext(_options))
            {
                var item = context.Items
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Name == "ItemOne");
                
                item.Name = "NewName";
                context.Items.Update(item);

                removeFunc();

                Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
            }
        }
    }
}