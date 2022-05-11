using System.Data.Common;
using BenchmarkDotNet.Attributes;
using EF.Tests.Common;
using EF.Tests.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EF.Benchmarks;

public class FirstOrDefaultOnPrimaryKeyBench 
{
    private readonly DbContextOptions<TestDbContext> _options;

    public FirstOrDefaultOnPrimaryKeyBench()
    {
        DbConnection connection = new SqliteConnection("Filename=:memory:");
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;
            
        connection.Open();
            
        using (var context = new TestDbContext(_options))
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
        using (var context = new TestDbContext(_options))
        {
            return context.Items.FirstOrDefault(x => x.Id == 5555);
        }
    }

    [Benchmark]
    public Item Find()
    {
        using (var context = new TestDbContext(_options))
        {
            return context.Find<Item>(5555);
        }
    }
}