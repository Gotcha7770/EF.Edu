using System;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Tests.Common;

public static class TestDbContextFactory
{
    public static readonly DbContextOptions<TestDbContext> LocalPostgresDbOptions =
        new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("host=localhost;database=testdb;user id=postgres;password=postgres;")
            .LogTo(Console.WriteLine)
            .Options;

    public static TestDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return Create(options, true);
    }

    public static TestDbContext Create(DbContextOptions<TestDbContext> options, bool isInMemory = false)
    {
        var context = new TestDbContext(options);
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        if (!isInMemory)
        {
            context.Database.ExecuteSqlRaw(
                """
                CREATE FUNCTION is_time_between(timestamp, time, time)
                RETURNS boolean
                RETURN $1::time without time zone between $2 and $3;
                """);
        }

        return context;
    }
}