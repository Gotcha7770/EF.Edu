using System;
using System.Linq.Expressions;
using EF.Tests.Interfaces;
using EntityFrameworkCore.Projectables;

namespace EF.Tests.Model;

public class Person : IEntity<int>
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
    
    public static Expression<Func<Person, string>> GetFullNameExpression { get; } = acc => string.Join(' ',
        string.IsNullOrWhiteSpace(acc.LastName) ? null : acc.LastName.Trim(),
        string.IsNullOrWhiteSpace(acc.FirstName) ? null : acc.FirstName.Trim(),
        string.IsNullOrWhiteSpace(acc.SecondName) ? null : acc.SecondName.Trim());
}