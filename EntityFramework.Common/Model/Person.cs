using System.Linq.Expressions;
using EntityFramework.Common.Interfaces;
using EntityFrameworkCore.Projectables;

namespace EntityFramework.Common.Model;

public class Person : IEntity<int>
{
    public int Id { get; init; }
    public string FirstName { get; init; }
    public string SecondName { get; init; }
    public string LastName { get; init; }
    public string CountryCode { get; init; }

    public Company Company { get; init; }

    [Projectable]
    public string FullName => SecondName == null 
        ? FirstName + " " + LastName
        : FirstName + " " + SecondName + " " + LastName;
    
    public static Expression<Func<Person, string>> GetFullNameExpression { get; } = acc => string.Join(
        ' ',
        string.IsNullOrWhiteSpace(acc.LastName) ? null : acc.LastName.Trim(),
        string.IsNullOrWhiteSpace(acc.FirstName) ? null : acc.FirstName.Trim(),
        string.IsNullOrWhiteSpace(acc.SecondName) ? null : acc.SecondName.Trim());
}