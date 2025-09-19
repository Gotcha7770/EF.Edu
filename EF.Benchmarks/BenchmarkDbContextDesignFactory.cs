using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EF.Benchmarks;

internal class BenchmarkDbContextDesignFactory : IDesignTimeDbContextFactory<BenchmarkDbContext>
{
    public BenchmarkDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseSqlite(BenchmarkDbContext.InMemoryConnectionString)
            .Options;

        return new BenchmarkDbContext(options);
    }

    public static BenchmarkDbContext CreateDbContext(DbConnection connection)
    {
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseSqlite(connection)
            .Options;

        return new BenchmarkDbContext(options);
    }
    
    public static BenchmarkDbContext CreateDbContext(DbContextOptions<BenchmarkDbContext> options)
    {
        var context =  new BenchmarkDbContext(options);
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        return context;
    }
}