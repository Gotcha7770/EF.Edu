using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EF.Tests.Common;

public static class DbContextExtensions
{
    public static async Task<T> AddFake<T>(this TestDbContext dbContext, Func<T> fakeFactory) where T : class
    {
        var fake = fakeFactory();
        await dbContext.AddAsync(fake);
        await dbContext.SaveChangesAsync();

        return fake;
    }

    public static IQueryable<T> Where<T, TProperty>(
        this IQueryable<T> source,
        Expression<Func<T, TProperty>> propertyAccessor,
        Expression<Func<TProperty, bool>> propertyPredicate)
    {
        return source.Where(propertyAccessor.Compose(propertyPredicate));
    }
}