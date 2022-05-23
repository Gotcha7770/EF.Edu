using EF.Tests.Model;
using Microsoft.EntityFrameworkCore;

namespace EF.Tests.Common
{
    public class TestDbContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        
        public DbSet<Document> Documents { get; set; }
        
        public DbSet<Person> Persons { get; set; }

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

            modelBuilder.Entity<Document>()
                .HasOne(x => x.Item)
                .WithOne()
                .HasForeignKey<Document>(x => x.ItemId)
                .IsRequired();

            // modelBuilder.Entity<Person>()
            //     .Property(x => x.FullName)
            //     //.HasComputedColumnSql(@"TRIM(COALESCE(""FirstName"", '') || ' ' || COALESCE(""SecondName"", '') || ' ' || COALESCE(""LastName"", ''))", stored: true);
            //     .HasComputedColumnSql(@"CASE WHEN ""SecondName"" IS NULL THEN ""FirstName"" || ' ' || ""LastName"" 
            //                                   ELSE ""FirstName"" || ' ' || ""SecondName"" || ' ' || ""LastName"" END", stored: true);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}