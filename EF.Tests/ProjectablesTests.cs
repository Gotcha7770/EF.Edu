using System;
using System.Linq;
using EF.Tests.Common;
using EF.Tests.Model;
using EntityFrameworkCore.Projectables;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

public class ProjectablesTests
{
    private readonly TestDbContext _dbContext;

    public ProjectablesTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseNpgsql("host=localhost;database=testdb;user id=postgres;password=postgres;")
            .UseProjectables()
            .LogTo(Console.WriteLine)
            .Options;

        _dbContext = TestDbContextFactory.Create(options);
    }

    [Fact]
    public void ProjectableProperty()
    {
        _dbContext.Persons.AddRange(
            new Person
            {
                FirstName = "Петр",
                LastName = "Петрович"
            },
            new Person
            {
                FirstName = "Иван",
                SecondName = "Иванов",
                LastName = "Иванович"
            });

        _dbContext.SaveChanges();

        var query = _dbContext.Persons.OrderBy(x => x.FullName);
        var sql = query.ToQueryString();
        var result = query.ToArray();
    }

    [Fact]
    public void ProjectableExtension()
    {
        // Trip[] items =
        // [
        //     new Trip
        //     {
        //         Points =
        //         [
        //             new Point
        //             {
        //                 Code = "First",
        //                 DepartureDate = new DateOnly(2024, 6, 11),
        //                 DepartureTime = new TimeOnly(9, 0)
        //             },
        //             new Point
        //             {
        //                 Code = "Second",
        //                 DepartureDate = new DateOnly(2024, 6, 12),
        //                 DepartureTime = new TimeOnly(10, 0)
        //             },
        //             new Point
        //             {
        //                 Code = "Third",
        //                 DepartureDate = new DateOnly(2024, 6, 13),
        //                 DepartureTime = new TimeOnly(11, 0)
        //             }
        //         ]
        //     },
        //     new Trip
        //     {
        //         Points =
        //         [
        //             new Point
        //             {
        //                 Code = "Third",
        //                 DepartureDate = new DateOnly(2024, 6, 13),
        //                 DepartureTime = new TimeOnly(10, 0)
        //             },
        //             new Point
        //             {
        //                 Code = "First",
        //                 DepartureDate = new DateOnly(2024, 6, 11),
        //                 DepartureTime = new TimeOnly(12, 0)
        //             }
        //         ]
        //     }
        // ];

        // _dbContext.Trips.AddRange(items);
        // _dbContext.SaveChanges();

        // var criterion = new Route(
        //     new Point { Code = "First", DepartureDate = new DateOnly(2024, 6, 11) },
        //     new Point { Code = "Third", DepartureDate = new DateOnly(2024, 6, 13) });

        // var query = _dbContext.Trips
        //     .Where(x => x.Start().Code == criterion.Start.Code
        //                 && x.StartDate() == criterion.Start.DepartureDate
        //                 && x.End().Code == criterion.End.Code);

        // var query =
        //     from trip in _dbContext.Trips
        //     let startPoint = trip.Start()
        //     where startPoint.DepartureDate == criterion.Start.DepartureDate
        //           && startPoint.Code == criterion.Start.Code
        //     select trip;
        //
        // var sql = query.ToQueryString();
        // var result = query.ToArray();
    }
}

public static partial class AdHocExtensions
{
    // [Projectable] 
    // public static Point Start(this Trip trip) => trip.Points.OrderBy(x => x.DepartureDate).First();
    //
    // [Projectable] 
    // public static Point End(this Trip trip) => trip.Points.OrderByDescending(x => x.DepartureDate).First();
}