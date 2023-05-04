using System.Threading.Tasks;

namespace EF.Tests.Common;

public static class DbContextExtensions
{
    public static async Task<T> AddFake<T>(this TestDbContext dbContext) where T : class
    {
        var fake = Fakes.Get<T>();
        await dbContext.AddAsync(fake);

        return fake;
    }
}