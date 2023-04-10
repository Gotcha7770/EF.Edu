using System.Collections.Generic;

namespace EF.Tests.Model;

public class Company
{
    public int Id { get; init; }
    public string Name { get; set; }
    
    public ICollection<Person> Persons { get; set; } = new List<Person>();
}