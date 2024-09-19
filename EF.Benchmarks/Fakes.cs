using Bogus;
using EF.Benchmarks.Entities;
using Person = EF.Benchmarks.Entities.Person;

namespace EF.Benchmarks;

public static class Fakes
{
    public static Faker<Person> PersonFaker { get; } = new Faker<Person>()
        .RuleFor(x => x.Id, f => f.IndexFaker++)
        .RuleFor(x => x.FirstName, f => f.Person.FirstName)
        .RuleFor(x => x.LastName, f => f.Person.LastName);

    public static Faker<Product> ProductFaker { get; } = new Faker<Product>()
        .RuleFor(x => x.Id, f => f.IndexFaker++);

    public static Faker<Sale> SaleFaker { get; } = new Faker<Sale>()
        .RuleFor(x => x.Id, f => f.IndexFaker++)
        .RuleFor(x => x.Customer, _ => PersonFaker)
        .RuleFor(x => x.CustomerId, (_, obj) => obj.Customer.Id)
        .RuleFor(x => x.Product, _ => ProductFaker)
        .RuleFor(x => x.ProductId, (_, obj) => obj.ProductId)
        .RuleFor(x => x.SaleDate, f => f.Date.Recent());
}