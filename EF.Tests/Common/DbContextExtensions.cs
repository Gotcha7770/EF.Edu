using System;
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
}