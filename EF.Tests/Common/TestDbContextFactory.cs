﻿using System;
using Microsoft.EntityFrameworkCore;

namespace EF.Tests.Common;

public static class TestDbContextFactory
{
    public static readonly DbContextOptions<TestDbContext> LocalPostgresDbOptions = new DbContextOptionsBuilder<TestDbContext>()
        .UseNpgsql("host=localhost;database=testdb;user id=postgres;password=postgres;")
        .LogTo(Console.WriteLine)
        .Options;

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

        context.Database.ExecuteSqlRaw(
            """
            CREATE FUNCTION is_time_between(time, time, time)
            RETURNS boolean
            RETURN $1 between $2 and $3;
            """);

        return context;
    }
}