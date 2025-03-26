using System.Linq.Expressions;

namespace vnvt_back_end.Infrastructure.Extensions
{
    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return ApplyOrder(source, propertyName, "OrderBy");
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return ApplyOrder(source, propertyName, "OrderByDescending");
        }

        private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string propertyName, string methodName)
        {
            var parameter = Expression.Parameter(typeof(T), "p");
            var property = Expression.Property(parameter, propertyName);
            var lambda = Expression.Lambda(property, parameter);

            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type);

            return (IOrderedQueryable<T>)method.Invoke(null, new object[] { source, lambda });
        }
    }

}
