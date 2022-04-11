using System;
using System.Linq;
using EF.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace EF.Tests;

public class TrackingTests
{
    [Fact]
    public void UpdateNotTracking()
    {
        var dbContext = TestDbContextFactory.Create();
        dbContext.Items.Add(new Item { Name = "OldName" });
        dbContext.SaveChanges();
        
        var item = dbContext.Items.AsNoTracking()
            .FirstOrDefault(x => x.Id == 1);

        item.Name = "NewName";
        Should.Throw<InvalidOperationException>(() => dbContext.Items.Update(item));
    }

    [Fact]
    public void ConcurrentAccess()
    {
        var dbContext1 = TestDbContextFactory.Create();
        dbContext1.Items.Add(new Item { Name = "OldName" });
        dbContext1.SaveChanges();

        var item1 = dbContext1.Find<Item>(1);
        item1.Name = "NewName";
        dbContext1.Items.Update(item1);
        
        var dbContext2 = TestDbContextFactory.Create();
        var item2 = dbContext1.Find<Item>(1);
        dbContext2.Items.Remove(item2);
        
        Should.Throw<DbUpdateConcurrencyException>(() => dbContext2.SaveChanges());
    }
}