using System.Collections.Generic;

namespace EF.Tests.Model;

public class Company
{
    public int Id { get; init; }
    public string Name { get; init; }
    
    public Address Address { get; init; }
    public ICollection<Person> Persons { get; init; } = [];
}