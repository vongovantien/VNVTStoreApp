using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace vnvt_back_end.Core.General
{
    /// <summary>
    ///
    /// </summary>
    public static class EntityExpression
    {
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IQueryable<T> OrderByDescendingExpression<T>(IQueryable<T> source, string propertyName)
        {
            var OrderByMethod =
                typeof(Queryable).GetMethods()
             .Where(method => method.Name == "OrderByDescending")
             .Single(method => method.GetParameters().Length == 2);
            //.Single();
            ParameterExpression parameter = Expression.Parameter(typeof(T), "c");
            Expression orderByProperty = Expression.Property(parameter, propertyName);

            LambdaExpression lambda = Expression.Lambda(orderByProperty, new[] { parameter });

            MethodInfo genericMethod = OrderByMethod.MakeGenericMethod
                (new[] { typeof(T), orderByProperty.Type });
            object ret = genericMethod.Invoke(null, new object[] { source, lambda });
            return (IQueryable<T>)ret;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IQueryable<T> OrderByAscendingExpression<T>(IQueryable<T> source, string propertyName)
        {
            var OrderByMethod =
                typeof(Queryable).GetMethods()
             .Where(method => method.Name == "OrderBy")
             .Where(method => method.GetParameters().Length == 2)
             .Single();
            ParameterExpression parameter = Expression.Parameter(typeof(T), "c");
            Expression orderByProperty = Expression.Property(parameter, propertyName);

            LambdaExpression lambda = Expression.Lambda(orderByProperty, new[] { parameter });

            MethodInfo genericMethod = OrderByMethod.MakeGenericMethod
                (new[] { typeof(T), orderByProperty.Type });
            object ret = genericMethod.Invoke(null, new object[] { source, lambda });
            return (IQueryable<T>)ret;
        }
    }
}