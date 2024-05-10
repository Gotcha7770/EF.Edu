using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EF.Tests.Common;

public static class ExpressionExtensions
{
    public static async Task AddIfNotExists<TEntity>(
        this TestDbContext dbContext,
        TEntity entity,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        await dbContext.Upsert(entity)
            .NoUpdate()
            .RunAsync(cancellationToken);
    }
}