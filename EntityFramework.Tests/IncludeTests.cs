using System.Threading.Tasks;
using EntityFramework.Common.Model;
using EntityFramework.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFramework.Tests;

public class IncludeTests
{
    [Fact]
    public async Task Test()
    {
        await using var dbContext = TestDbContextFactory.Create();
        
        var microsoft = new Company { Name = "Microsoft" };
        var google = new Company { Name = "Google" };
        dbContext.Companies.AddRange(microsoft, google);
        
        var tom = new Person { FirstName = "Tom", Company = microsoft };
        var bob = new Person { FirstName = "Bob", Company = google };
        var alice = new Person { FirstName = "Alice", Company = microsoft };
        var kate = new Person { FirstName = "Kate" };
        dbContext.Persons.AddRange(tom, bob, alice, kate);
                 
        await dbContext.SaveChangesAsync();

        var persons = await dbContext.Persons
            .Include(x => x.Company)
            .ToArrayAsync();
        
        var companies = await dbContext.Companies
            .Include(x => x.Persons)
            .ToArrayAsync();
    }
}