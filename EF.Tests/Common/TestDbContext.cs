using EF.Tests.Model;
using Microsoft.EntityFrameworkCore;

namespace EF.Tests.Common
{
    public class TestDbContext : DbContext
    {
        public DbSet<Item> Items { get; set; }

        public TestDbContext() { }
        
        public TestDbContext(DbContextOptions<TestDbContext> contextOptions)
            : base(contextOptions)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("OrderNumbers");

            modelBuilder.Entity<Item>()
                .Property(x => x.Order)
                .HasDefaultValueSql("nextval('\"OrderNumbers\"')");

            modelBuilder.Entity<Item>()
                .Property(x => x.Computed)
                .HasComputedColumnSql(@"""Name"" || ':' || ""Amount""", stored: true);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}