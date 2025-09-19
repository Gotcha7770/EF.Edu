using EF.Benchmarks.Entities;
using EntityFramework.Common.Model;
using Microsoft.EntityFrameworkCore;

namespace EF.Benchmarks;

public class BenchmarkDbContext : DbContext
{
    public const string InMemoryConnectionString = "Filename=:memory:";
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Segment> Segments => Set<Segment>();

    public BenchmarkDbContext(DbContextOptions<BenchmarkDbContext> options) : base(options)
    {
    }
}