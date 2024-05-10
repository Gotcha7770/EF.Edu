using System.Linq;
using System.Threading.Tasks;
using EF.Tests.Common;
using EF.Tests.Model;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace EF.Tests;

public class InDbMappingTests
{
    // https://www.npgsql.org/efcore/mapping/translations.html#string-functions

    [Theory]
    [InlineData("Иван", "Иваныч", "Иванов", "Иванов Иван Иваныч")]
    [InlineData("", "Иваныч", "Иванов", "Иванов Иваныч")]
    [InlineData("Иван", "Иваныч", " ", "Иван Иваныч")]
    [InlineData("Иван", null, "Иванов", "Иванов Иван")]
    public void StringJoinTest(
        string firstName,
        string secondName,
        string lastName,
        string expected)
    {
        string Selector(Person x) => string.Join(' ',
            string.IsNullOrWhiteSpace(x.LastName) ? null : x.LastName.Trim(),
            string.IsNullOrWhiteSpace(x.FirstName) ? null : x.FirstName.Trim(),
            string.IsNullOrWhiteSpace(x.SecondName) ? null : x.SecondName.Trim());

        string result = Selector(new Person
        {
            FirstName = firstName,
            SecondName = secondName,
            LastName = lastName
        });
        
        result.Should().Be(expected);
    }
    
    [Theory]
    [InlineData("Иван", "Иваныч", "Иванов", "Иванов Иван Иваныч")]
    [InlineData("", "Иваныч", "Иванов", "Иванов Иваныч")]
    [InlineData("Иван", "Иваныч", " ", "Иван Иваныч")]
    [InlineData("Иван", null, "Иванов", "Иванов Иван")]
    public async Task Test(
        string firstName,
        string secondName,
        string lastName,
        string expected)
    {
        var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        await dbContext.AddAsync(new Person
        {
            FirstName = firstName,
            SecondName = secondName,
            LastName = lastName
        });
        await dbContext.SaveChangesAsync();

        var query = dbContext.Persons.AsNoTracking()
            .Select(Person.GetFullNameExpression);

        string result = await query.FirstOrDefaultAsync();

        result.Should().Be(expected);
    }
}