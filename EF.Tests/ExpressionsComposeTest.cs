using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EF.Tests.Common;
using EF.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

public class ExpressionsComposeTest
{
    private static readonly string[] Companies = ["WB", ""];

    [Fact]
    public async Task Compose()
    {
        var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);

        var company = await dbContext.AddFake(() =>
            Fakes.CompanyFaker
                .WithPersons(Fakes.Get<Person>())
                .Generate());

        //Expression<Func<Company, bool>> companySpec = x => x.Name == "WB";
        Expression<Func<Company, bool>> companySpec = x => !Companies.Contains(x.Name);
        Expression<Func<Person, bool>> personSpec = x => x.Company.Name == company.Name;

        var companyQuery = dbContext.Companies.Where(companySpec);
        //var personQuery = dbContext.Persons.Where(x => x.Company.Name != "ADT" && x.Company.Name != "MMM");
        // var personQuery = dbContext.Persons
        //     .Where(x => x.Company.Address.City != "Moscow")
        //     .Where(x => x.Company.Address.City != "Tokyo");
        //var personQuery = dbContext.Persons.Where(x => companySpec(x.Company)); can't do this
        var personQuery = dbContext.Persons.Where(x => x.Company, companySpec);

        var sql = personQuery.ToQueryString();
        var result = await personQuery.ToArrayAsync();
    }
}