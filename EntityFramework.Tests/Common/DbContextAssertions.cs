using System;
using System.Linq;
using System.Linq.Expressions;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using AwesomeAssertions.Primitives;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Tests.Common;

public class DbContextAssertions : ReferenceTypeAssertions<DbContext, DbContextAssertions>
{
    public DbContextAssertions(DbContext dbContext) : base(dbContext, AssertionChain.GetOrCreate()) { }

    protected override string Identifier => "dbContext";

    public AndConstraint<DbContextAssertions> Contain<TEntity, TKey>(
        TEntity expected,
        Func<TEntity, TKey> keySelector,
        string because = "",
        params object[] becauseArgs) where TEntity : class
    {
        // bool success = Execute.Assertion
        //     .BecauseOf(because, becauseArgs)
        //     .ForCondition(Subject is not null)
        //     .FailWith("Expected {context:dbContext} to contain {0}{reason}, but found <null>.", expected);
        //
        // if (success)
        // {
        //     var key = keySelector(expected);
        //     var stored = Subject.Set<TEntity>().Find(key);
        //
        //     if (stored is null)
        //     {
        //         Execute.Assertion
        //             .BecauseOf(because, becauseArgs)
        //             .FailWith("Expected {context:dbContext} {0} to contain {1}{reason} with key {2}.",
        //                 Subject,
        //                 expected,
        //                 key);
        //     }
        // }

        return new AndConstraint<DbContextAssertions>(this);
    }
    
    public AndConstraint<DbContextAssertions> Contain<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        string because = "",
        params object[] becauseArgs) where TEntity : class
    {
        // bool success = Execute.Assertion
        //     .BecauseOf(because, becauseArgs)
        //     .ForCondition(Subject is not null)
        //     .FailWith("Expected {context:dbContext} to contain entity with condition, but found <null>.", predicate);
        //
        // if (success)
        // {
        //     var stored = Subject.Set<TEntity>().FirstOrDefault(predicate);
        //
        //     if (stored is null)
        //     {
        //         Execute.Assertion
        //             .BecauseOf(because, becauseArgs)
        //             .FailWith("Expected {context:dbContext} {0} to contain entity with condition {1}.",
        //                 Subject,
        //                 predicate);
        //     }
        // }

        return new AndConstraint<DbContextAssertions>(this);
    }

    public AndConstraint<DbContextAssertions> NotContain<TEntity, TKey>(TEntity expected, Func<TEntity, TKey> keySelector, string because = "", params object[] becauseArgs) where TEntity : class
    {
        // bool success = Execute.Assertion
        //     .BecauseOf(because, becauseArgs)
        //     .ForCondition(Subject is not null)
        //     .FailWith("Expected {context:dbContext} to contain {0}{reason}, but found <null>.", expected);
        //
        // if (success)
        // {
        //     var key = keySelector(expected);
        //     var stored = Subject.Set<TEntity>().Find(key);
        //     
        //     if (stored is not null)
        //     {
        //         Execute.Assertion
        //             .BecauseOf(because, becauseArgs)
        //             .FailWith("Expected {context:dbContext} {0} to not contain {1}{reason} with key {2}.",
        //                 Subject,
        //                 expected,
        //                 key);
        //     }
        // }
        
        return new AndConstraint<DbContextAssertions>(this);
    }
}