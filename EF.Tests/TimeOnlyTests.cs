﻿using System;
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
    private readonly TestDbContext _dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);

    [Fact]
    public void TimeInRange()
    {
        Trip[] items =
        [
            new Trip
            {
                Legs =
                [
                    new Leg
                    {
                        StartCode = "First",
                        DepartureDate = new DateOnly(2024, 6, 11),
                        EndCode = "Second",
                        ArrivalDate = new DateOnly(2024, 6, 12)
                    },
                    new Leg
                    {
                        StartCode = "Second",
                        DepartureDate = new DateOnly(2024, 6, 12),
                        EndCode = "Third",
                        ArrivalDate = new DateOnly(2024, 6, 13)
                    }
                ]
            },
            new Trip
            {
                Legs =
                [
                    new Leg
                    {
                        StartCode = "Second",
                        DepartureDate = new DateOnly(2024, 6, 11),
                        EndCode = "First",
                        ArrivalDate = new DateOnly(2024, 6, 13)
                    }
                ]
            }
        ];

        _dbContext.Trips.AddRange(items);
        _dbContext.SaveChanges();

        var start = new TimeOnly(10, 0);
        var end = new TimeOnly(11, 0);

        var raw = _dbContext.Legs.FromSql(
            $"""
            select * from "Points"
            where "DepartureTime" between {start} and {end}
            """).ToArray();
        
        var query = _dbContext.Trips
            .Where(x => TestDbContext.IsTimeBetween(x.Legs.Select(p => p.DepartureTime).Min(), start, end));
            
        var sql = query.ToQueryString();
        var result = query.ToArray();
    }
}