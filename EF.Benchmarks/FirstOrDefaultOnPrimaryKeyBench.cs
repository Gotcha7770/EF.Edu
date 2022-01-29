using System.Data.Common;
using BenchmarkDotNet.Attributes;
using EF.Tests;
using EF.Tests.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EF.Benchmarks;

public class FirstOrDefaultOnPrimaryKeyBench 
{
    private readonly DbContextOptions<TestContext> _options;
    private readonly DbConnection _connection;
    
    public FirstOrDefaultOnPrimaryKeyBench()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _options = new DbContextOptionsBuilder<TestContext>()
            .UseSqlite(_connection)
            .Options;
            
        _connection.Open();
            
        using (var context = new TestContext(_options))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var items = Enumerable.Range(0, 10000)
                .Select(x => new Item
                {
                    Id = x,
                    Name = $"Item{x}",
                    Amount = 0
                });
            context.Items.AddRange(items);
            context.SaveChanges();
        }
    }
    
    [Benchmark]
    public Item FirstOrDefault()
    {
        using (var context = new TestContext(_options))
        {
            return context.Items.FirstOrDefault(x => x.Id == 5555);
        }
    }

    [Benchmark]
    public Item Find()
    {
        using (var context = new TestContext(_options))
        {
            return context.Find<Item>(5555);
        }
    }
}