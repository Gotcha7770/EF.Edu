using System;
using System.Linq;
using EF.Tests.Common;
using EF.Tests.Model;
using EntityFrameworkCore.MemoryJoin;
using Maddalena;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

public class InMemoryMappingTests
{
    record PersonDto(string FirstName, string Country);

    [Fact]
    public void JoinInMemoryCollectionTest()
    {
        var countries = Enum.GetValues<CountryCode>()
            .Select(x => (Code: x.ToString(), Country: Country.FromCode(x).OfficialName))
            .ToArray();

        var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        dbContext.Persons.AddRange(
            new Person
            {
                FirstName = "Петр",
                CountryCode = "CZ"
            },
            new Person
            {
                FirstName = "Иван",
                CountryCode = "JP"
            },
            new Person
            {
                FirstName = "Сергей",
                CountryCode = "JP"
            },
            new Person
            {
                FirstName = "Евгений",
                CountryCode = "AU"
            });

        dbContext.SaveChanges();

        var queryable = dbContext.FromLocalList(countries);

        var query = dbContext.Persons.Join(queryable,
                l => l.CountryCode,
                r => r.Code,
                (l, r) => new { l.FirstName, r.Country })
            .Where(x => x.Country == "Japan")
            .OrderBy(x => x.FirstName)
            .Skip(1)
            .Take(5);
        
        var sql = query.ToQueryString();
        var result = query.ToArray();
    }
}