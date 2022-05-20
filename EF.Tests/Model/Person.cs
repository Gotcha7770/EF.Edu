namespace EF.Tests.Model;

public class Person
{
    public int Id { get; init; }

    public string FirstName { get; set; }
    
    public string SecondName { get; set; }
    
    public string LastName { get; set; }

    public string FullName { get; set; }
}