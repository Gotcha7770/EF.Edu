using System;
using System.Linq;
using System.Threading.Tasks;
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
        _dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
    }

    [Fact]
    public async Task TimeInRange()
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

        await _dbContext.Trips.AddRangeAsync(items);
        await _dbContext.SaveChangesAsync();

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
        var result = await query.ToArrayAsync();
    }
}