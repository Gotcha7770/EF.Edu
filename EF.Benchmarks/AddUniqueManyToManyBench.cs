using BenchmarkDotNet.Attributes;
using EntityFramework.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EF.Benchmarks;

[MemoryDiagnoser]
public class AddUniqueManyToManyBench
{
    private readonly DbContextOptions<BenchmarkDbContext> _options = new DbContextOptionsBuilder<BenchmarkDbContext>()
        .UseNpgsql("host=localhost;database=bench-db;user id=postgres;password=postgres;")
        .Options;

    private BenchmarkDbContext _context;

    private readonly Route _route = new Route
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

    [GlobalSetup]
    public void Setup()
    {
        using var context = BenchmarkDbContextDesignFactory.CreateDbContext(_options);

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // var items = Fakes.SaleFaker.GenerateLazy(ItemsCount);
        // context.Sales.AddRange(items);
        // context.SaveChanges();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _context = BenchmarkDbContextDesignFactory.CreateDbContext(_options);
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Dispose();
    }
    
    [Benchmark]
    public async Task<Route> With_Tracking()
    {
        var existed = _context.Segments
            .Where(x => _route.Segments.Any(s => s.SegmentKey == x.SegmentKey))
            .Select(x => x.SegmentKey);

        var entry = await _context.Routes.AddAsync(_route);
        return entry.Entity;
    }
    
    [Benchmark]
    public async Task<Route> UsingRawSQL()
    {
        var entry = await AddUsingRawSql(_context, _route);
        
        return entry.Entity;
    }
    
    private async Task<EntityEntry<Route>> AddUsingRawSql(BenchmarkDbContext dbContext, Route route)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        // вставляем сам маршрут
        string insertRouteSql =
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

        // вставляем сегменты
        foreach (var segment in route.Segments)
        {
            string insertSegmentSql =
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

            // вставляем связь в join-таблицу (название уточни у себя в БД, EF генерит RouteSegment/RouteSegments)
            string insertJoinSql =
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