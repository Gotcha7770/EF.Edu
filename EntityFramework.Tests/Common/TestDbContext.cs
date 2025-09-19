using System;
using EntityFramework.Common.Model;
using EntityFrameworkCore.MemoryJoin;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Tests.Common;

public class TestDbContext : DbContext
{
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Segment> Segments => Set<Segment>();

    protected DbSet<QueryModelClass> QueryData => Set<QueryModelClass>();

    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> contextOptions)
        : base(contextOptions)
    {
    }

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

        modelBuilder.Entity<Route>()
            .HasKey(x => x.Id);
        
        modelBuilder.Entity<Route>()
            .Property(x => x.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Route>()
            .HasMany(e => e.Segments)
            .WithMany();

        modelBuilder.Entity<Segment>()
            .HasKey(x => new { x.Carrier, x.FlightNumber, x.DepartureDate });

        // modelBuilder.Entity<Segment>()
        //     .HasIndex(x => new { x.Carrier, x.FlightNumber, x.DepartureDate })
        //     .IsUnique();

        base.OnModelCreating(modelBuilder);
    }

    public static bool IsTimeBetween(DateTime dateTime, TimeOnly start, TimeOnly end) =>
        throw new NotImplementedException(); //time >= start && time <= end;
}