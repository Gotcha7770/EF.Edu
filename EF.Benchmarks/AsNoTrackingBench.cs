using BenchmarkDotNet.Attributes;
using EF.Benchmarks.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EF.Benchmarks;

[MemoryDiagnoser]
public class AsNoTrackingBench
{
    private SqliteConnection _connection;

    [Params(1, 100, 1000)]
    public int ItemsCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // База данных SQLite в памяти (Filename=:memory:) живет только в рамках одного соединения.
        _connection = new SqliteConnection(BenchmarkDbContext.InMemoryConnectionString);
        using var context = BenchmarkDbContextDesignFactory.CreateDbContext(_connection);
        _connection.Open();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var items = Fakes.SaleFaker.GenerateLazy(ItemsCount);
        context.Sales.AddRange(items);
        context.SaveChanges();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _connection.Close();
    }

    [Benchmark]
    public Task<Sale[]> With_Tracking()
    {
        using var context = BenchmarkDbContextDesignFactory.CreateDbContext(_connection);
        return context.Sales
            .ToArrayAsync();
    }

    [Benchmark]
    public Task<Sale[]> Without_Tracking()
    {
        using var context = BenchmarkDbContextDesignFactory.CreateDbContext(_connection);
        return context.Sales
            .AsNoTracking()
            .ToArrayAsync();
    }
}