namespace EF.Tests.Dtos;

public class PersonDto
{
    public int Id { get; init; }

    public string FirstName { get; set; }
    
    public string SecondName { get; set; }
    
    public string LastName { get; set; }

    public string FullName { get; set; }
    
    public string Country { get; set; }
}