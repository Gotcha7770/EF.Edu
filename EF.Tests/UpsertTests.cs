using System;
using System.Threading;
using System.Threading.Tasks;
using EF.Tests.Common;
using EF.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

public class UpsertTests
{
    [Fact]
    public async Task AddIfNotExists()
    {
        var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        await dbContext.AddIfNotExists(new Person { FirstName = "Person to replace" }, CancellationToken.None);
        
        dbContext.Should()
            .Contain<Person>(x => x.FirstName == "Person to replace");
    }
    
    [Fact]
    public async Task IfExistsNotAdd()
    {
        var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);
        var person = await dbContext.AddFake<Person>();
        
        await dbContext.AddIfNotExists(new Person { Id = person.Id, FirstName = "Person to replace" }, CancellationToken.None);
        dbContext.Should()
            .Contain<Person>(x => x.Id == person.Id && x.FirstName == person.FirstName);
    }
}