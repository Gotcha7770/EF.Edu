using System;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using EntityFramework.Common;
using EntityFramework.Common.Model;
using EntityFramework.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFramework.Tests;

public class ObjectGraphComparisonTests
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

    [Fact]
    public async Task NoCircularReferences()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        
        var result = await dbContext.AddAsync(_route1);
        await dbContext.SaveChangesAsync();

        var saved = dbContext.Routes
            .AsNoTracking()
            .Include(x => x.Segments)
            .FirstOrDefault();
        
        saved.Should()
            .BeEquivalentTo(_route1);
        saved.Should()
            .BeEquivalentTo(result.Entity);
        _route1.Should()
            .BeEquivalentTo(saved);
        _route1.Should()
            .BeEquivalentTo(result.Entity);
        result.Entity.Should()
            .BeEquivalentTo(_route1);
        result.Entity.Should()
            .BeEquivalentTo(saved);
    }

    [Fact]
    public async Task CircularReferencesWithChangeTracker()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        var company = Fakes.CompanyFaker
            .WithPersons(Fakes.Get<Person>())
            .Generate();
        
        var result = await dbContext.AddAsync(company);
        await dbContext.SaveChangesAsync();
        
        var saved = dbContext.Companies
            .AsNoTracking()
            .Include(x => x.Persons)
            .Include(x => x.Address)
            .FirstOrDefault();
        
        saved.Should()
            .BeEquivalentTo(company, options => options.IgnoringCyclicReferences());
        saved.Should()
            .BeEquivalentTo(result.Entity, options => options.IgnoringCyclicReferences());
        company.Should()
            .BeEquivalentTo(saved, options => options.IgnoringCyclicReferences());
        company.Should()
            .BeEquivalentTo(result.Entity, options => options.IgnoringCyclicReferences());
    }
    
    [Fact]
    public async Task CircularReferencesWithoutChangeTracker()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        var company = Fakes.Get<Company>();
        var person = Fakes.PersonFaker.WithCompany(company).Generate();

        var result = await dbContext.AddAsync(person);
        await dbContext.SaveChangesAsync();

        person = new Person
        {
            Id = person.Id,
            FirstName = person.FirstName,
            SecondName =  person.SecondName,
            LastName = person.LastName,
            CountryCode = person.CountryCode,
            Company = new Company
            {
                Id = company.Id,
                Name = company.Name,
                Address = new Address
                {
                    Id = company.Address.Id,
                    City = company.Address.City,
                    CountryCode = company.Address.CountryCode
                }
            }
        };
        
        //company.Persons.Clear(); // simulate untracked behavior

        var saved = dbContext.Persons
            .AsNoTracking()
            .Include(x => x.Company)
                .ThenInclude(x => x.Address)
            .FirstOrDefault();
       
        saved.Should()
            .BeEquivalentTo(person, options => options.Excluding(ctx => ctx.Path == "Company.Persons"));
        // saved.Should()
        //     .BeEquivalentTo(person, options => options.Excluding(x => x.Company.Persons));
        // saved.Should()
        //     .BeEquivalentTo(person, options => options.IgnoringCyclicReferences());
        // saved.Should()
        //     .BeEquivalentTo(result.Entity, options => options.IgnoringCyclicReferences());
        // person.Should()
        //     .BeEquivalentTo(saved, options => options.ExcludingNestedObjects());
        // person.Should()
        //     .BeEquivalentTo(result.Entity, options => options.IgnoringCyclicReferences());
    }
}