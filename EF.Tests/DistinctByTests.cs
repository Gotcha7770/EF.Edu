using System;
using System.Linq;
using EF.Tests.Common;
using EF.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

public class DistinctByTests
{
    [Fact]
    public void Test()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseNpgsql("host=localhost;database=testdb;user id=postgres;password=postgres;")
            .LogTo(Console.WriteLine)
            .Options;
        
        var dbContext = TestDbContextFactory.Create(options);
        
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
            //.Distinct();
            .DistinctBy(x => x.FirstName);
        
        var sql = query.ToQueryString();
        var result = query.ToArray();
    }
}