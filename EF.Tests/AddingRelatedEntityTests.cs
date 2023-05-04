using System.Threading.Tasks;
using EF.Tests.Common;
using EF.Tests.Model;
using Xunit;

namespace EF.Tests;

public class AddingRelatedEntityTests
{
    // https://learn.microsoft.com/en-us/ef/core/saving/related-data#adding-a-related-entity

    [Fact]
    public async Task AddOneToOneEntity()
    {
        var dbContext = TestDbContextFactory.Create();
        var document = await dbContext.AddFake<Document>();

        var item = Fakes.Get<Item>();
        document.Item = item;
        await dbContext.SaveChangesAsync();

        dbContext.Should().Contain(item, x => x.Id);
    }

    [Fact]
    public async Task AddManyToOneEntity()
    {
        var dbContext = TestDbContextFactory.Create();
        var company = await dbContext.AddFake<Company>();

        var person = Fakes.Get<Person>();
        company.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        dbContext.Should().Contain(person, x => x.Id);
    }
}