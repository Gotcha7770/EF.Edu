using System.Linq;
using EntityFramework.Common.Model;
using EntityFramework.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFramework.Tests;

public class DistinctByTests
{
    [Fact]
    public void Test()
    {
        var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        
        dbContext.Persons.AddRange(
            new Person
            {
                FirstName = "Петр",
                LastName = "Петров"
            },
            new Person
            {
                FirstName = "Иван"
            },
            new Person
            {
                FirstName = "Сергей"
            },
            new Person
            {
                FirstName = "Петр",
                LastName = "Остров"
            });
        
        dbContext.SaveChanges();

        var query = dbContext.Persons
            .DistinctBy(x => x.FirstName);
        
        var sql = query.ToQueryString();
        var result = query.ToArray();
    }
}