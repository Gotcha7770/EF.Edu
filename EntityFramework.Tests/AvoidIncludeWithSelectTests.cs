using System.Linq;
using System.Threading.Tasks;
using EntityFramework.Common.Model;
using EntityFramework.Tests.Common;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFramework.Tests;

public class AvoidIncludeWithSelectTests
{
    //is there any difference?

    [Fact]
    public async Task WithInclude()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        await dbContext.AddAsync(new Company
        {
            Name = "AWS",
            Persons =
            {
                new Person { FirstName = "Иван" },
                new Person { FirstName = "Петр" }
            }
        });
        await dbContext.SaveChangesAsync();

        var query = dbContext.Companies.AsNoTracking()
            .Include(x => x.Persons)
            .Select(x => new
            {
                x.Name,
                Stuff = string.Join(';', x.Persons.Select(p => p.FirstName))
            });

        // SELECT c."Name", c."Id", p."FirstName", p."Id"
        // FROM "Companies" AS c
        // LEFT JOIN "Persons" AS p ON c."Id" = p."CompanyId"
        // ORDER BY c."Id"

        query.ToArray().Should()
            .BeEquivalentTo([new { Name = "AWS", Stuff = "Иван;Петр" }]);
    }

    [Fact]
    public async Task WithoutInclude()
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        await dbContext.AddAsync(new Company
        {
            Name = "AWS",
            Persons =
            {
                new Person { FirstName = "Иван" },
                new Person { FirstName = "Петр" }
            }
        });

        var query = dbContext.Companies.AsNoTracking()
            .Select(x => new
            {
                x.Name,
                Stuff = string.Join(';', x.Persons.Select(p => p.FirstName))
            });

        // SELECT c."Name", c."Id", p."FirstName", p."Id"
        // FROM "Companies" AS c
        // LEFT JOIN "Persons" AS p ON c."Id" = p."CompanyId"
        // ORDER BY c."Id"
        
        query.ToArray().Should()
            .BeEquivalentTo([new { Name = "AWS", Stuff = "Иван;Петр" }]);
    }
}