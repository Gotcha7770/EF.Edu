using System.Linq;
using System.Threading.Tasks;
using EntityFramework.Common.Model;
using EntityFramework.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFramework.Tests;

public class EnumerateTests
{
    [Fact]
    public async Task Test()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        
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