using System;
using Microsoft.EntityFrameworkCore;

namespace EF.Tests.Common;

public static class TestDbContextFactory
{
    public static TestDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return Create(options);
    }

    public static TestDbContext Create(DbContextOptions<TestDbContext> options)
    {
        var context = new TestDbContext(options);

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        return context;
    }
}