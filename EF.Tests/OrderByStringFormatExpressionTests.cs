using System.Linq;
using EF.Tests.Common;
using EF.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

public class OrderByStringFormatExpressionTests
{
    [Fact]
    public void Test()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseNpgsql("host=localhost;database=testdb;user id=postgres;password=postgres;")
            .Options;

        var dbContext = TestDbContextFactory.Create(options);
        dbContext.Persons.AddRange(
            new Person
            {
                FirstName = "Петр",
                LastName = "Петрович"
            },
            new Person
            {
                FirstName = "Иван",
                SecondName = "Иванов",
                LastName = "Иванович"
            });

        dbContext.SaveChanges();

        var result = dbContext.Persons
            //.OrderBy(x => $"{x.FirstName ?? string.Empty} {x.SecondName ?? string.Empty} {x.LastName ?? string.Empty}")
            .OrderBy(x => x.FullName)
            .ToArray();
    }
}