using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;
using AwesomeAssertions;
using EntityFramework.Tests.Common;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Xunit;

namespace EntityFramework.Tests;

public class EntityWithPeriod
{
    public int Id { get; init; }
    public required NpgsqlRange<DateTime> Period { get; init; }
}

public class RangeTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<EntityWithPeriod> Entities { get; set; }
    }

    private readonly TestDbContext _dbContext;

    public RangeTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("host=localhost;database=testdb;user id=postgres;password=postgres;")
            .Options;
        
        _dbContext = new TestDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
    }

    [Theory]
    [ClassData(typeof(RangeTestCases))]
    public void TestInRange(NpgsqlRange<DateTime> period, DateTime from, DateTime till, bool expected)
    {
        // [ [period] ]
        // var start = new DateTime(2025, 05, 01,  0, 0, 0, DateTimeKind.Utc);
        // var end = new DateTime(2025, 06, 01,  0, 0, 0, DateTimeKind.Utc);
        _dbContext.Add(new EntityWithPeriod { Period = period });
        _dbContext.SaveChanges();

        // var from = start.AddDays(1);
        // var till = end.AddDays(-1);
        //var range =  new NpgsqlRange<DateTime>(from, till);
        // var result = _dbContext.Entities
        //     .InPeriod(from, till)
        //     .Where(x => x.Period.ContainedBy(new NpgsqlRange<DateTime>(from, till)))
        //     .Any();

        var result = period.IsContainedBy(new NpgsqlRange<DateTime>(from, till));

        //result.Should().NotBeNull();
        result.Should().Be(expected);
    }
}

interface ISpecification<in T> where T : class
{
    bool IsSatisfiedBy(T entity);
    Func<T, bool> Compile();
}

public static partial class AdHocExtensions
{
    public static IQueryable<EntityWithPeriod> InPeriod(
        this IQueryable<EntityWithPeriod> mixOrdersQuery, 
        DateTime? startDateUtc,
        DateTime? endDateUtc)
    {
        return mixOrdersQuery.Where(mixOrder => (startDateUtc == null ||
                                                 mixOrder.Period.LowerBoundInfinite ||
                                                 (mixOrder.Period.LowerBoundIsInclusive
                                                     ? startDateUtc <= mixOrder.Period.LowerBound
                                                     : startDateUtc < mixOrder.Period.LowerBound)) &&
                                                (endDateUtc == null ||
                                                 mixOrder.Period.UpperBoundInfinite ||
                                                 (mixOrder.Period.UpperBoundIsInclusive
                                                     ? endDateUtc >= mixOrder.Period.LowerBound
                                                     : endDateUtc > mixOrder.Period.LowerBound)));
    }

    public static bool IsContainedBy(this NpgsqlRange<DateTime> one, NpgsqlRange<DateTime> other)
    {
        return one.LowerBound >= other.LowerBound 
               && one.LowerBound <= other.UpperBound;
    }
}

public class RangeTestCases : TheoryData<NpgsqlRange<DateTime>, DateTime, DateTime, bool>
{
    public RangeTestCases()
    {
        var period = new NpgsqlRange<DateTime>(
            new DateTime(2025, 05, 01, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 06, 01, 0, 0, 0, DateTimeKind.Utc));
        Add(
            period,
            period.LowerBound.AddDays(-1),
            period.UpperBound.AddDays(1),
            true);
        Add(
            period, 
            period.LowerBound.AddDays(1),
            period.UpperBound.AddDays(-1),
            false);
        Add(
            period, 
            period.LowerBound,
            period.UpperBound,
            true);
        Add(
            period, 
            period.LowerBound.AddDays(1),
            period.UpperBound.AddDays(1),
            false);
        Add(
            period, 
            period.LowerBound.AddDays(-1),
            period.UpperBound.AddDays(-1),
            false);
    }
}