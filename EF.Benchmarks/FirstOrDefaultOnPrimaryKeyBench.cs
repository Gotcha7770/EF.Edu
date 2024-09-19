using BenchmarkDotNet.Attributes;
using EF.Benchmarks.Entities;
using Microsoft.Data.Sqlite;

namespace EF.Benchmarks;

[MemoryDiagnoser]
public class FirstOrDefaultOnPrimaryKeyBench
{
    private SqliteConnection _connection;
    private const int PersonId = 5555;

    [GlobalSetup]
    public void Setup()
    {
        // База данных SQLite в памяти (Filename=:memory:) живет только в рамках одного соединения.
        _connection = new SqliteConnection(BenchmarkDbContext.InMemoryConnectionString);
        using var context = BenchmarkDbContextDesignFactory.CreateDbContext(_connection);
        _connection.Open();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var items = Fakes.SaleFaker.GenerateLazy(10000);
        context.Sales.AddRange(items);
        context.SaveChanges();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _connection.Close();
    }

    [Benchmark]
    public Sale FirstOrDefault()
    {
        using var context = BenchmarkDbContextDesignFactory.CreateDbContext(_connection);
        return context.Sales.FirstOrDefault(x => x.Id == PersonId);
    }

    [Benchmark]
    public Sale Find()
    {
        using var context = BenchmarkDbContextDesignFactory.CreateDbContext(_connection);
        return context.Find<Sale>(PersonId);
    }
}