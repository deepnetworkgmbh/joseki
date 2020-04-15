using System;
using System.Linq;
using System.Linq.Expressions;

namespace webapp.Models
{
    /// <summary>
    /// A Linq predicate builder to support dynamic query concatenation.
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// returns a true expression.
        /// </summary>
        /// <typeparam name="T">Type used by the expression.</typeparam>
        /// <returns>true.</returns>
        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        /// <summary>
        /// returns a false expression.
        /// </summary>
        /// <typeparam name="T">Type used by the expression.</typeparam>
        /// <returns>false.</returns>
        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        /// <summary>
        /// Invokes an OR operator using the provided expresion with parameters.
        /// </summary>
        /// <typeparam name="T">Type used by the expression.</typeparam>
        /// <returns>boolean.</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        /// <summary>
        /// Invokes an AND operator using the provided expresion with parameters.
        /// </summary>
        /// <typeparam name="T">Type used by the expression.</typeparam>
        /// <returns>boolean.</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
