using EF.Benchmarks.Entities;
using Microsoft.EntityFrameworkCore;

namespace EF.Benchmarks;

public class BenchmarkDbContext : DbContext
{
    public const string InMemoryConnectionString = "Filename=:memory:";

    public DbSet<Person> Persons { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Sale> Sales { get; set; }

    public BenchmarkDbContext(DbContextOptions<BenchmarkDbContext> options) : base(options)
    {
    }
}