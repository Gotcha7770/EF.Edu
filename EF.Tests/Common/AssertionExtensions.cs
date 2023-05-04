using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;

namespace EF.Tests.Common;

public static class AssertionExtensions
{
    [Pure]
    public static DbContextAssertions Should(this DbContext dbContext)
    {
        return new DbContextAssertions(dbContext);
    }
}