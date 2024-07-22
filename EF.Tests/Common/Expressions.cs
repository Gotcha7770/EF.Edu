using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EF.Tests.Common;

// https://habr.com/ru/articles/313394/

public static class Expressions
{
    public static Expression<Func<T, TConverted>> Convert<T, TResult, TConverted>(this Expression<Func<T, TResult>> expr)
    {
        return Expression.Lambda<Func<T, TConverted>>(
            Expression.Convert(expr, typeof(TConverted)), expr.Parameters);
    }

    public static Expression<Func<T, bool>> Not<T>(Expression<Func<T, bool>> expr)
    {
        return Expression.Lambda<Func<T, bool>>(Expression.Not(expr.Body), expr.Parameters.Single());
    }

    public static Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        return left.Compose(right, Expression.AndAlso);
    }

    public static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        return left.Compose(right, Expression.OrElse);
    }

    // public static Expression<Func<TIn, TOut>> Compose<TIn, TInOut, TOut>(
    //     this Expression<Func<TIn, TInOut>> input,
    //     Expression<Func<TInOut, TOut>> inOutOut)
    // {
    //     var parameter = Expression.Parameter(typeof(TIn));
    //
    //     return Expression.Lambda<Func<TIn, TOut>>(
    //         Expression.Invoke(
    //             inOutOut,
    //             Expression.Invoke(input, parameter)), 
    //         parameter);
    // }
    
    /// <summary>
    /// Combines the first expression with the second using the specified merge function.
    /// </summary>
    public static Expression<T> Compose<T>(
        this Expression<T> first, 
        Expression<T> second,
        Func<Expression, Expression, Expression> merge)
    {
        // zip parameters (map from parameters of second to parameters of first)
        var map = first.Parameters
            .Select((x, i) => new { First = x, Second = second.Parameters[i] })
            .ToDictionary(x => x.Second, p => p.First);
 
        // create a merged lambda expression with parameters from the first expression
        return Expression.Lambda<T>(
            merge(first.Body, ParameterRebinder.ReplaceParameters(map, second.Body)), 
            first.Parameters);
    }

    public static Expression<Func<TIn, TOut>> Compose<TIn, TIntermediate, TOut>(
        this Expression<Func<TIn, TIntermediate>> propertyAccessor,
        Expression<Func<TIntermediate, TOut>> predicate)
    {
        // Expression expression = predicate.Body switch
        // {
        //     BinaryExpression binaryExpression => binaryExpression.Update(
        //         (binaryExpression.Left as MemberExpression).Wrap(propertyAccessor.Body),
        //         binaryExpression.Conversion,
        //         binaryExpression.Right),
        //     MethodCallExpression methodCallExpression => methodCallExpression.Update(
        //         methodCallExpression.Object,
        //         [
        //             methodCallExpression.Arguments[0],
        //             (methodCallExpression.Arguments[1] as MemberExpression).Wrap(propertyAccessor.Body)
        //         ])
        // };

        var expression = new PropertyEditor(typeof(TIntermediate), x => x.Wrap(propertyAccessor.Body)).Visit(predicate);
        return Expression.Lambda<Func<TIn, TOut>>(
            (expression as LambdaExpression).Body,
            propertyAccessor.Parameters[0]);
    }

    public static MemberExpression Wrap(this MemberExpression first, Expression second)
    {
        return Expression.Property(second, first.Member.Name);
    }

    public class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        private ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameter(LambdaExpression exp, ParameterExpression parameter)
        {
            return ReplaceParameters(
                new Dictionary<ParameterExpression, ParameterExpression>
                {
                    [exp.Parameters[0]] = parameter
                },
                exp);
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }
 
        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            if (_map.TryGetValue(parameter, out var replacement))
            {
                parameter = replacement;
            }
 
            return base.VisitParameter(parameter);
        }
    }

    public class PropertyEditor : ExpressionVisitor
    {
        private readonly Type _entityType;
        private readonly Func<MemberExpression, MemberExpression> _selector;

        public PropertyEditor(Type entityType, Func<MemberExpression, MemberExpression> selector)
        {
            _entityType = entityType;
            _selector = selector;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is ParameterExpression parameter && parameter.Type == _entityType)
            {
                return _selector(node);
            }

            return base.VisitMember(node);
        }
    }
}