namespace EntityFramework.Common.Model;

public class Company
{
    public int Id { get; init; }
    public string Name { get; init; }
    
    public Address Address { get; init; }
    public ICollection<Person> Persons { get; init; } = [];
}