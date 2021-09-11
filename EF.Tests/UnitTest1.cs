using System.Data.Common;
using System.Linq;
using EF.Tests.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EF.Tests
{
    public class Tests
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

                var one = new Item(1, "ItemOne");
                var two = new Item(2, "ItemTwo");
                var three = new Item(3, "ItemThree");
                
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
        public void Can_get_items()
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
    }
}