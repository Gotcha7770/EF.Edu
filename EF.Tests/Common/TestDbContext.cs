using System;
using EF.Tests.Model;
using EntityFrameworkCore.MemoryJoin;
using Microsoft.EntityFrameworkCore;

namespace EF.Tests.Common;

public class TestDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Leg> Legs { get; set; }
        
    protected DbSet<QueryModelClass> QueryData { get; set; }

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

        modelBuilder.Entity<Trip>()
            .HasMany(x => x.Legs)
            .WithOne(x => x.Trip)
            .HasForeignKey(x => x.TripId);
        
        modelBuilder.Entity<Leg>()
            .Property(x => x.DepartureDate)
            .HasColumnType("timestamp without time zone");
            
        modelBuilder.Entity<Leg>()
            .Property(x => x.ArrivalDate)
            .HasColumnType("timestamp without time zone");

        modelBuilder.HasDbFunction(typeof(TestDbContext).GetMethod(nameof(IsTimeBetween), [typeof(DateTime), typeof(TimeOnly), typeof(TimeOnly)]))
            .HasName("is_time_between");

        base.OnModelCreating(modelBuilder);
    }

    public static bool IsTimeBetween(DateTime dateTime, TimeOnly start, TimeOnly end) =>
        throw new NotImplementedException(); //time >= start && time <= end;
}