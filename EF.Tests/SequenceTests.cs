using EF.Tests.Common;
using EF.Tests.Model;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

public class SequenceTests
{

    [Fact]
    public void AddItem_AfterRemoveAnother_OrderIsIncremented()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("host=localhost;database=testdb;user id=postgres;password=postgres;")
            .Options;
        
        var dbContext = TestDbContextFactory.Create(options);
        dbContext.Items.AddRange(new Item
            {
                Amount = 12
            },
            new Item
            {
                Amount = 22
            },
            new Item
            {
                Amount = 32
            });

        dbContext.SaveChanges();

        var item = dbContext.Find<Item>(2);
        dbContext.Items.Remove(item);
        dbContext.SaveChanges();

        dbContext.Items.Add(new Item { Amount = 42 });
        dbContext.SaveChanges();
        
        dbContext.Find<Item>(4).Order.Should().Be(4);
    }
}