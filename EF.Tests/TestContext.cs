using EF.Tests.Model;
using Microsoft.EntityFrameworkCore;

namespace EF.Tests
{
    public class TestContext : DbContext
    {
        public DbSet<Item> Items { get; set; }

        public TestContext(DbContextOptions<TestContext> contextOptions)
            : base(contextOptions)
        { }
    }
}