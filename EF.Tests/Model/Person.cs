using EntityFrameworkCore.Projectables;

namespace EF.Tests.Model;

public class Person
{
    public int Id { get; init; }

    public string FirstName { get; set; }
    
    public string SecondName { get; set; }
    
    public string LastName { get; set; }
    
    public string CountryCode { get; set; }

    public Company Company { get; set; }

    [Projectable]
    public string FullName => SecondName == null 
        ? FirstName + " " + LastName
        : FirstName + " " + SecondName + " " + LastName;
}