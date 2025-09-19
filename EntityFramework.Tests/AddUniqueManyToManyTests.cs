using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EFCore.BulkExtensions;
using EntityFramework.Common.Model;
using EntityFramework.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;
using static AwesomeAssertions.FluentActions;

namespace EntityFramework.Tests;

// https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many

public class AddUniqueManyToManyTests
{
    private readonly Route _route1 = new Route
    {
        Id = Guid.NewGuid(),
        StartCode = "LED",
        EndCode = "AER",
        Segments =
        [
            new Segment
            {
                StartCode = "LED",
                DepartureDate = new DateOnly(2025, 08, 30),
                EndCode = "MOW",
                ArrivalDate = new DateOnly(2025, 08, 30),
                Carrier = "SU",
                FlightNumber = "2345"
            },
            new Segment
            {
                StartCode = "MOW",
                DepartureDate = new DateOnly(2025, 08, 30),
                EndCode = "AER",
                ArrivalDate = new DateOnly(2025, 08, 31),
                Carrier = "DP",
                FlightNumber = "0017"
            }
        ]
    };

    private readonly Route _route2 = new Route
    {
        Id = Guid.NewGuid(),
        StartCode = "LED",
        EndCode = "KZN",
        Segments =
        [
            new Segment
            {
                StartCode = "LED",
                DepartureDate = new DateOnly(2025, 08, 30),
                EndCode = "MOW",
                ArrivalDate = new DateOnly(2025, 08, 30),
                Carrier = "SU",
                FlightNumber = "2345"
            },
            new Segment
            {
                StartCode = "MOW",
                DepartureDate = new DateOnly(2025, 08, 31),
                EndCode = "KZN",
                ArrivalDate = new DateOnly(2025, 08, 31),
                Carrier = "SU",
                FlightNumber = "2198"
            }
        ]
    };

    [Fact]
    public async Task AddDuplicate_UsingEF_ThrowsException()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);

        var result = await dbContext.AddAsync(_route1);
        await dbContext.SaveChangesAsync();

        await Invoking(async () =>
        {
            result = await dbContext.AddAsync(_route2);
            await dbContext.SaveChangesAsync();
        }).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AddDuplicate_UsingFind()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);

        var result = await dbContext.AddAsync(_route1);
        await dbContext.SaveChangesAsync();

        dbContext.AddOrUpdateRange(
            _route2.Segments,
            x => [x.Carrier, x.FlightNumber, x.DepartureDate]);
        result = await dbContext.AddAsync(_route2);
        //result = await dbContext.AddOrUpdateAsync(_route2.Segments[0]);
        await dbContext.SaveChangesAsync();

        result.Entity.Should().BeEquivalentTo(_route2);
        
        dbContext.Routes
            .Should()
            .BeEquivalentTo([_route1, _route2], opt => opt.Excluding(x => x.Segments));

        dbContext.Segments
            .Should()
            .BeEquivalentTo(new HashSet<Segment>([.._route1.Segments, .._route2.Segments]));
    }

    [Fact]
    public async Task AddDuplicate_UsingRawSQL()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);

        var result = await dbContext.AddAsync(_route1);
        await dbContext.SaveChangesAsync();

        result = await AddUsingRawSql(dbContext, _route2);

        result.Entity.Should().BeEquivalentTo(_route2);

        dbContext.Routes
            .Should()
            .BeEquivalentTo([_route1, _route2], opt => opt.Excluding(x => x.Segments));

        dbContext.Segments
            .Should()
            .BeEquivalentTo(new HashSet<Segment>([.._route1.Segments, .._route2.Segments]));
    }

    [Fact]
    public async Task AddDuplicate_UsingBulkInsert()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);

        var result = await dbContext.AddAsync(_route1);
        await dbContext.SaveChangesAsync();

        // https://entityframework-extensions.net/bulk-insert#common-options-in-entity-framework-extensions
        // https://github.com/videokojot/EFCore.BulkExtensions.MIT

        await dbContext.BulkInsertOrUpdateAsync(_route2.Segments, options =>
        {
            // options.OnConflictUpdateWhereColumns = new[] { "Carrier", "FlightNumber", "DepartureDate" };
            // options.IgnoreOnConflict = true; // чтобы не вставлять дубликаты
        });
        
        dbContext.Detach(_route2.Segments);
        result = await dbContext.AddAsync(_route2);
        foreach (var segment in _route2.Segments)
        {
            dbContext.Entry(segment).State = EntityState.Unchanged;
        }
        
        await dbContext.SaveChangesAsync();
        
        result.Entity.Should().BeEquivalentTo(_route2);

        dbContext.Routes
            .Should()
            .BeEquivalentTo([_route1, _route2], opt => opt.Excluding(x => x.Segments));

        dbContext.Segments
            .Should()
            .BeEquivalentTo(new HashSet<Segment>([.._route1.Segments, .._route2.Segments]));
    }

    private async Task<EntityEntry<Route>> AddUsingRawSql(TestDbContext dbContext, Route route)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        const string insertRouteSql =
            """
                INSERT INTO "Routes" ("Id", "StartCode", "DepartureDate", "EndCode")
                VALUES ({0}, {1}, {2}, {3})
            """;

        await dbContext.Database.ExecuteSqlRawAsync(
            insertRouteSql,
            route.Id,
            route.StartCode,
            route.DepartureDate,
            route.EndCode);

        foreach (var segment in route.Segments)
        {
            const string insertSegmentSql =
                """
                    INSERT INTO "Segments" ("Carrier", "FlightNumber", "DepartureDate", "StartCode", "ArrivalDate", "EndCode")
                    VALUES ({0}, {1}, {2}, {3}, {4}, {5})
                    ON CONFLICT ("Carrier", "FlightNumber", "DepartureDate") DO NOTHING;
                """;

            await dbContext.Database.ExecuteSqlRawAsync(
                insertSegmentSql,
                segment.Carrier,
                segment.FlightNumber,
                segment.DepartureDate,
                segment.StartCode,
                segment.ArrivalDate,
                segment.EndCode);

            const string insertJoinSql =
                """
                    INSERT INTO "RouteSegment" ("RouteId", "SegmentsCarrier", "SegmentsFlightNumber", "SegmentsDepartureDate")
                    VALUES ({0}, {1}, {2}, {3})
                """;

            await dbContext.Database.ExecuteSqlRawAsync(
                insertJoinSql,
                route.Id,
                segment.Carrier,
                segment.FlightNumber,
                segment.DepartureDate);
        }

        await transaction.CommitAsync();

        return dbContext.Entry(route);
    }
}

public static partial class AdHocExtensions
{
    /// <summary>
    /// Открепляет сущность от ChangeTracker.
    /// </summary>
    public static void Detach<TEntity>(this DbContext dbContext, IEnumerable<TEntity> entities)
        where TEntity : class
    {
        foreach (var entity in entities)
        {
            foreach (var tracked in dbContext.ChangeTracker.Entries<TEntity>())
            {
                if (tracked.Entity.Equals(entity))
                {
                    tracked.State = EntityState.Detached;
                }
            }
        }
        // var tracked = dbContext.ChangeTracker.Entries<TEntity>().ToArray();
        // var entry = dbContext.Entry(entities);
        // if (entry.State != EntityState.Detached)
        // {
        //     entry.State = EntityState.Detached;
        // }
    }

    public static EntityEntry<TEntity> AddOrUpdateAsync<TEntity>(
        this DbContext dbContext,
        TEntity entity,
        Func<TEntity, object[]> keyValuesSelector,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var keyValues = keyValuesSelector(entity);
        // var attached = await dbContext.FindAsync<TEntity>(keyValues, cancellationToken);
        // if (attached is null)
        // {
        //     return await dbContext.AddAsync(entity, cancellationToken);
        // }

        // Проверяем, не трекается ли уже объект
        var tracked = dbContext.ChangeTracker
            .Entries<TEntity>()
            .FirstOrDefault(e => e.Properties
                .Select(p => p.CurrentValue)
                .SequenceEqual(keyValues));

        if (tracked != null)
        {
            return tracked;
        }

        // Просто "прикрепляем пустышку"
        dbContext.Attach(entity);

        return dbContext.Entry(entity);

        //return dbContext.Entry(attached);
    }

    public static void AddOrUpdateRange<TEntity>(
        this DbContext dbContext,
        IEnumerable<TEntity> entities,
        Func<TEntity, object[]> keyValuesSelector,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        foreach (var entity in entities)
        {
            var entry = AddOrUpdateAsync(dbContext, entity, keyValuesSelector, cancellationToken);
            //dbContext.Entry(entity).State = EntityState.Unchanged;
        }
    }
}