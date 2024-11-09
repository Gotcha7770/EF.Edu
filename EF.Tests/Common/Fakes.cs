using System;
using Bogus;
using EF.Tests.Model;
using Person = EF.Tests.Model.Person;

namespace EF.Tests.Common;

internal static class Fakes
{
    public static Faker<Item> ItemFaker { get; } = new Faker<Item>()
        .RuleFor(x => x.Id, f => f.IndexFaker)
        .RuleFor(x => x.Order, f => f.Random.Int())
        .RuleFor(x => x.Amount, f => f.Random.Int());

    public static Faker<Document> DocumentFaker { get; } = new Faker<Document>()
        .RuleFor(x => x.Id, f => f.IndexFaker);
    
    public static Faker<Person> PersonFaker { get; } = new Faker<Person>()
        .RuleFor(x => x.Id, f => f.IndexFaker)
        .RuleFor(x => x.FirstName, f => f.Person.FirstName)
        .RuleFor(x => x.SecondName, f => f.Person.FirstName)
        .RuleFor(x => x.LastName, f => f.Person.LastName);
    
    public static Faker<Company> CompanyFaker { get; } = new Faker<Company>()
        .RuleFor(x => x.Id, f => f.IndexFaker)
        .RuleFor(x => x.Name, f => f.Company.CompanyName());

    public static Faker<Trip> TripFaker { get; } = new Faker<Trip>();
        
    public static T Get<T>() where T : class => Get(typeof(T)) as T;
    
    private static object Get(Type type)
    {
        return type switch
        {
            Type when type == typeof(Item) => ItemFaker.Generate(),
            Type when type == typeof(Document) => DocumentFaker.Generate(),
            Type when type == typeof(Person) => PersonFaker.Generate(),
            Type when type == typeof(Company) => CompanyFaker.Generate(),
            Type when type == typeof(Trip) => TripFaker.Generate(),
            _ => throw new ArgumentOutOfRangeException(type.Name)
        };
    }
    
    public static Faker<Company> WithPersons(this Faker<Company> source, params Person[] persons)
    {
        source = source.Clone();
        return source.RuleFor(x => x.Persons, persons);
    }
}