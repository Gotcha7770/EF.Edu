using System.Linq;
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
        await using var dbContext = TestDbContextFactory.Create();

        var document = await dbContext.AddFake(Fakes.Get<Document>);

        dbContext.Should().Contain(document, x => x.Id);
    }

    [Fact]
    public async Task AddManyToOneEntity()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var company = await dbContext.AddFake<Company>(
            Fakes.CompanyFaker
                .WithPersons(Fakes.Get<Person>()));

        dbContext.Should().Contain(company.Persons.First(), x => x.Id);
    }
}