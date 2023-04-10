using System;
using System.Linq;
using System.Threading.Tasks;
using EF.Tests.Common;
using EF.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

public class EnumerateTests
{
    [Fact]
    public async Task Test()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseNpgsql("host=localhost;database=testdb;user id=postgres;password=postgres;")
            .LogTo(Console.WriteLine)
            .Options;
        
        var dbContext = TestDbContextFactory.Create(options);
        
        var microsoft = new Company { Name = "Microsoft" };
        var google = new Company { Name = "Google" };
        dbContext.Companies.AddRange(microsoft, google);
        
        var tom = new Person { FirstName = "Tom", Company = microsoft };
        var bob = new Person { FirstName = "Bob", Company = google };
        var alice = new Person { FirstName = "Alice", Company = microsoft };
        var kate = new Person { FirstName = "Kate" };
        dbContext.Persons.AddRange(tom, bob, alice, kate);
        
        await dbContext.SaveChangesAsync();

        var query = dbContext.Persons
            .Include(x => x.Company)
            .GroupBy(x => x.Company)
            .Select(x => x.Key)
            .AsAsyncEnumerable();

        //NpgsqlDataReader in there
        await using (var enumarator = query.GetAsyncEnumerator())
        {
            while (await enumarator.MoveNextAsync())
            {
                var company = enumarator.Current;
            }
        }
    }
}