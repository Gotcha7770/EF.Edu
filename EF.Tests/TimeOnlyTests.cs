using System;
using System.Linq;
using EF.Tests.Common;
using EF.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

// https://learn.microsoft.com/ru-ru/ef/core/querying/sql-queries
// https://learn.microsoft.com/en-us/ef/core/querying/user-defined-function-mapping

public class TimeOnlyTests
{
    private readonly TestDbContext _dbContext;

    public TimeOnlyTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseNpgsql("host=localhost;database=testdb2;user id=postgres;password=postgres;")
            .UseProjectables()
            .LogTo(Console.WriteLine)
            .Options;

        _dbContext = TestDbContextFactory.Create(options);
    }

    [Fact]
    public void TimeInRange()
    {
        Trip[] items =
        [
            new Trip
            {
                Points =
                [
                    new Point
                    {
                        Code = "First",
                        DepartureDate = new DateOnly(2024, 6, 11),
                        DepartureTime = new TimeOnly(9, 0)
                    },
                    new Point
                    {
                        Code = "Second",
                        DepartureDate = new DateOnly(2024, 6, 12),
                        DepartureTime = new TimeOnly(10, 0)
                    },
                    new Point
                    {
                        Code = "Third",
                        DepartureDate = new DateOnly(2024, 6, 13),
                        DepartureTime = new TimeOnly(11, 0)
                    }
                ]
            },
            new Trip
            {
                Points =
                [
                    new Point
                    {
                        Code = "Third",
                        DepartureDate = new DateOnly(2024, 6, 13),
                        DepartureTime = new TimeOnly(10, 0)
                    },
                    new Point
                    {
                        Code = "First",
                        DepartureDate = new DateOnly(2024, 6, 11),
                        DepartureTime = new TimeOnly(12, 0)
                    }
                ]
            }
        ];

        _dbContext.Trips.AddRange(items);
        _dbContext.SaveChanges();

        var start = new TimeOnly(10, 0);
        var end = new TimeOnly(11, 0);

        var raw = _dbContext.Points.FromSql(
            $"""
            select * from "Points"
            where "DepartureTime" between {start} and {end}
            """).ToArray();
        
        var query = _dbContext.Trips
            .Where(x => TestDbContext.IsTimeBetween(x.Points.Select(p => p.DepartureTime).Min(), start, end));
            
        var sql = query.ToQueryString();
        var result = query.ToArray();
    }
}