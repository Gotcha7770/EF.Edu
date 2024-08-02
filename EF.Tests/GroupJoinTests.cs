using System;
using System.Linq;
using EF.Tests.Common;
using EF.Tests.Model;
using EntityFrameworkCore.MemoryJoin;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

public class GroupJoinTests
{
    private class Route
    {
        public string StartCode { get; init; }
        public DateOnly DepartureDate { get; init; }
        public string EndCode { get; init; }
        public DateOnly ArrivalDate { get; init; }
    }

    private readonly TestDbContext _dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);

    [Fact]
    public void GroupJoin()
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

        Route[] creteria =
        [
            new Route
            {
                StartCode = "First",
                DepartureDate = new DateOnly(2024, 6, 11),
                EndCode = "Third",
                ArrivalDate = new DateOnly(2024, 6, 13)
            }
        ];

        var routes = _dbContext.FromLocalList(creteria);

        var query =
            from trip in _dbContext.Trips
            join point in _dbContext.Legs on trip.Id equals point.TripId into pointsByTrip
            let start = pointsByTrip.OrderBy(x => x.DepartureDate).First()
            let end = pointsByTrip.OrderBy(x => x.ArrivalDate).Last()
            join route in routes
                on new { start.StartCode, start.DepartureDate }
                equals new { route.StartCode, route.DepartureDate }
            select new { trip, pointsByTrip };
        
        var sql = query.ToQueryString();

        var result = query.ToArray();
    }
}