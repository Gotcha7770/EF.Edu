using System.Threading.Tasks;
using EF.Tests.Common;
using EF.Tests.Model;
using FluentAssertions;
using Xunit;

namespace EF.Tests;

public class SequenceTests
{
    [Fact]
    public async Task AddItem_AfterRemoveAnother_OrderIsIncremented()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
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

        await dbContext.SaveChangesAsync();

        var item = await dbContext.FindAsync<Item>(2);
        dbContext.Items.Remove(item);
        await dbContext.SaveChangesAsync();

        dbContext.Items.Add(new Item { Amount = 42 });
        await dbContext.SaveChangesAsync();

        var result = await dbContext.FindAsync<Item>(4);

        result.Order.Should().Be(4);
    }
}