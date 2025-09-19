using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityFramework.Common;
using EntityFramework.Common.Model;
using EntityFramework.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFramework.Tests;

public class ExpressionsComposeTest
{
    [Fact]
    public async Task Compose()
    {
        var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);

        var company = await dbContext.AddFake(
            Fakes.CompanyFaker
                .WithPersons(Fakes.Get<Person>()));

        //Expression<Func<Company, bool>> companySpec = x => x.Name == "WB";
        //Expression<Func<Company, bool>> companySpec = x => !Companies.Contains(x.Name);
        Expression<Func<Company, bool>> companySpec = x => x.Address.City != "Moscow";
        // Expression<Func<Person, bool>> personSpec = x => x.Company.Name == company.Name;  <- duplicate!

        var companyQuery = dbContext.Companies.Where(companySpec);
        // var personQuery = dbContext.Persons.Where(x => companySpec(x.Company)); <- can't do this
        var personQuery = dbContext.Persons.Where(x => x.Company, companySpec);

        var sql = personQuery.ToQueryString();
        var result = await personQuery.ToArrayAsync();
    }
}