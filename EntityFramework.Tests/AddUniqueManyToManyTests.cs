using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EntityFramework.Common.Model;
using EntityFramework.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public async Task AddDuplicate_UsingChangeTracker()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);

        var result = await dbContext.AddAsync(_route1);
        await dbContext.SaveChangesAsync();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (var segment in _route2.Segments)
            {
                await dbContext.Database.ExecuteSqlAsync(
                    $"""
                         INSERT INTO "Segments" 
                         ("StartCode", "DepartureDate", "EndCode", "ArrivalDate", "Carrier", "FlightNumber")
                         VALUES ({segment.StartCode}, {segment.DepartureDate}, {segment.EndCode}, {segment.ArrivalDate}, {segment.Carrier}, {segment.FlightNumber})
                         ON CONFLICT ("Carrier", "FlightNumber", "DepartureDate") DO NOTHING;
                     """);

                var tracked = dbContext.ChangeTracker.Entries<Segment>()
                    .FirstOrDefault(e =>
                        e.Entity.Carrier == segment.Carrier &&
                        e.Entity.FlightNumber == segment.FlightNumber &&
                        e.Entity.DepartureDate == segment.DepartureDate);
                
                if (tracked != null)
                {
                    tracked.State = EntityState.Detached;
                }

                //dbContext.Detach(segment);
                dbContext.Entry(segment).State = EntityState.Unchanged;
            }

            result = dbContext.Routes.Add(_route2);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
        }

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

    private async Task<EntityEntry<Route>> AddUsingRawSql(TestDbContext dbContext, Route route)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
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

                const string insertRouteToSegmentSql =
                    """
                        INSERT INTO "RouteSegment" ("RouteId", "SegmentsCarrier", "SegmentsFlightNumber", "SegmentsDepartureDate")
                        VALUES ({0}, {1}, {2}, {3})
                    """;

                await dbContext.Database.ExecuteSqlRawAsync(
                    insertRouteToSegmentSql,
                    route.Id,
                    segment.Carrier,
                    segment.FlightNumber,
                    segment.DepartureDate);
            }

            await transaction.CommitAsync();
            
            return dbContext.Entry(route);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

public static partial class AdHocExtensions
{
    /// <summary>
    /// Открепляет сущность от ChangeTracker.
    /// </summary>
    public static void Detach<TEntity>(this DbContext dbContext, params TEntity[] entities)
        where TEntity : class
    {
        foreach (var entity in entities)
        {
            // var tmp = dbContext.Find<TEntity>("1");
            // var manager = dbContext.GetService<IStateManager>();
            foreach (var tracked in dbContext.ChangeTracker.Entries<TEntity>())
            {
                if (tracked.Entity.Equals(entity))
                {
                    tracked.State = EntityState.Detached;
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Наверно, эффективный, но не безопасный способ получить затреканный элемент из DbContext
    /// </summary>
    /// <param name="db"></param>
    /// <param name="keyValues"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static EntityEntry<TEntity> TryGetTracked<TEntity>(this DbContext db, params object[] keyValues)
        where TEntity : class
    {
        var entry = TryGetTracked(db, typeof(TEntity), keyValues);
        return entry is null
            ? null
            : new EntityEntry<TEntity>(entry);
    }

    public static InternalEntityEntry TryGetTracked(this DbContext db, Type type, params object[] keyValues)
    {
        var stateManager = db.GetService<IStateManager>();
        var entityType = db.Model.FindEntityType(type);
        if (entityType is null)
            throw new InvalidOperationException($"Entity type {type.Name} not found in the model.");
        var key = entityType.FindPrimaryKey();
        var entry = stateManager.TryGetEntry(key, keyValues);
        return entry;
    }

    public static EntityEntry<TEntity> AddOrUpdateAsync<TEntity>(
        this DbContext dbContext,
        TEntity entity,
        Func<TEntity, object[]> keyValuesSelector,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var keyValues = keyValuesSelector(entity);

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