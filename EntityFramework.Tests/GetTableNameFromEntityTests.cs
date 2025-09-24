using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using EntityFramework.Common.Model;
using EntityFramework.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EntityFramework.Tests;

public class GetTableNameFromEntityTests
{
    [Theory]
    [InlineData(typeof(Person), "Persons")]
    [InlineData(typeof(Company), "Companies")]
    public async Task GetTableNameFromEntity(Type type, string expected)
    {
        await using var dbContext = TestDbContextFactory.Create(TestDbContextFactory.LocalPostgresDbOptions);

        dbContext.GetTableName(type).Should().Be(expected);
    }
}

public static partial class AdHocExtensions
{
    public static string GetTableName<TEntity>(this DbContext context) where TEntity : class
    {
        return context.GetTableName(typeof(TEntity));
    }
    
    public static string GetTableName(this DbContext context, Type type)
    {
        //TODO: если Postgres
        IEntityType entityType = context.Model.FindEntityType(type);
        if (entityType is null)
            throw new InvalidOperationException($"Entity type {type.Name} not found in the model.");
        
        return entityType.GetTableName();
    }
}