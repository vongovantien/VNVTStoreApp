using FW.WAPI.Core.DAL.DTO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FW.WAPI.Core.General
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
        /// <param name="SearchFieldList"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetWhereExp<T>(List<SearchDTO> SearchFieldList, T item)
        {
            try
            {
                var pe = Expression.Parameter(item.GetType(), "c");

                Expression combined = null;

                if (SearchFieldList != null)
                {
                    foreach (var fieldItem in SearchFieldList)
                    {
                        var isArrayValue = JsonUtilities.CheckJsonArray(fieldItem.SearchValue);

                        var columnNameProperty = Expression.Property(pe, fieldItem.SearchField);

                        if (!isArrayValue)
                        {
                            var columnValue = fieldItem.SearchValue.GetType().FullName == "System.DateTime" ?
                                Expression.Constant(DateTime.Parse(fieldItem.SearchValue.ToString()), typeof(DateTime)) : Expression.Constant(fieldItem.SearchValue);

                            switch (fieldItem.SearchCondition)
                            {
                                case SearchCondition.Equal:
                                    var e1 = Expression.Equal(columnNameProperty, columnValue);
                                    combined = combined == null ? e1 : Expression.And(combined, e1);
                                    break;

                                case SearchCondition.NotEqual:
                                    var e2 = Expression.NotEqual(columnNameProperty, columnValue);
                                    combined = combined == null ? e2 : Expression.And(combined, e2);
                                    break;

                                case SearchCondition.GreaterThan:
                                    var e3 = Expression.GreaterThan(columnNameProperty, columnValue);
                                    combined = combined == null ? e3 : Expression.And(combined, e3);
                                    break;

                                case SearchCondition.GreaterThanEqual:
                                    var e4 = Expression.GreaterThanOrEqual(columnNameProperty, columnValue);
                                    combined = combined == null ? e4 : Expression.And(combined, e4);
                                    break;

                                case SearchCondition.LessThan:
                                    var e5 = Expression.LessThan(columnNameProperty, columnValue);
                                    combined = combined == null ? e5 : Expression.And(combined, e5);
                                    break;

                                case SearchCondition.LessThanEqual:
                                    var e6 = Expression.LessThanOrEqual(columnNameProperty, columnValue);
                                    combined = combined == null ? e6 : Expression.And(combined, e6);
                                    break;

                                case SearchCondition.Contains:
                                    var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                                    dynamic e7 = Expression.Call(columnNameProperty, method, columnValue);
                                    //(containsMethod, columnNameProperty, Expression.Constant(columnValue, typeof(string)));
                                    combined = combined == null ? e7 : Expression.And(combined, e7);
                                    break;

                                case SearchCondition.DateTimeRange:
                                    var e8 = Expression.GreaterThanOrEqual(columnNameProperty, columnValue);
                                    combined = combined == null ? e8 : Expression.And(combined, e8);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else
                        {
                            var arrayValue = ((JArray)fieldItem.SearchValue).ToObject<object[]>();

                            switch (fieldItem.SearchCondition)
                            {
                                case SearchCondition.Equal:

                                    Expression orCombine = null;

                                    foreach (var val in arrayValue)
                                    {
                                        var columnValue = val.GetType().FullName == "System.DateTime" ?
                                            Expression.Constant(DateTime.Parse(val.ToString()), typeof(DateTime)) : Expression.Constant(val);

                                        var e1 = Expression.Equal(columnNameProperty, columnValue);
                                        orCombine = orCombine == null ? e1 : Expression.Or(orCombine, e1);
                                    }
                                    combined = combined == null ? orCombine : Expression.And(combined, orCombine);
                                    //var e1 = Expression.Equal(columnNameProperty, columnValue);
                                    //combined = combined == null ? e1 : Expression.And(combined, e1);
                                    break;

                                case SearchCondition.NotEqual:
                                    Expression orCombine2 = null;

                                    foreach (var val in arrayValue)
                                    {
                                        var columnValue = val.GetType().FullName == "System.DateTime" ?
                                          Expression.Constant(DateTime.Parse(val.ToString()), typeof(DateTime)) : Expression.Constant(val);

                                        var e1 = Expression.NotEqual(columnNameProperty, columnValue);
                                        orCombine2 = orCombine2 == null ? e1 : Expression.Or(orCombine2, e1);
                                    }
                                    combined = combined == null ? orCombine2 : Expression.And(combined, orCombine2);
                                    break;

                                case SearchCondition.GreaterThan:
                                    Expression orCombine3 = null;

                                    foreach (var val in arrayValue)
                                    {
                                        var columnValue = val.GetType().FullName == "System.DateTime" ?
                                           Expression.Constant(DateTime.Parse(val.ToString()), typeof(DateTime)) : Expression.Constant(val);

                                        var e1 = Expression.GreaterThan(columnNameProperty, columnValue);
                                        orCombine3 = orCombine3 == null ? e1 : Expression.Or(orCombine3, e1);
                                    }
                                    combined = combined == null ? orCombine3 : Expression.And(combined, orCombine3);
                                    break;

                                case SearchCondition.GreaterThanEqual:
                                    Expression orCombine4 = null;

                                    foreach (var val in arrayValue)
                                    {
                                        var columnValue = val.GetType().FullName == "System.DateTime" ?
                                           Expression.Constant(DateTime.Parse(val.ToString()), typeof(DateTime)) : Expression.Constant(val);

                                        var e1 = Expression.GreaterThanOrEqual(columnNameProperty, columnValue);
                                        orCombine4 = orCombine4 == null ? e1 : Expression.Or(orCombine4, e1);
                                    }
                                    combined = combined == null ? orCombine4 : Expression.And(combined, orCombine4);
                                    break;

                                case SearchCondition.LessThan:
                                    Expression orCombine5 = null;
                                    foreach (var val in arrayValue)
                                    {
                                        var columnValue = val.GetType().FullName == "System.DateTime" ?
                                          Expression.Constant(DateTime.Parse(val.ToString()), typeof(DateTime)) : Expression.Constant(val);

                                        var e1 = Expression.LessThan(columnNameProperty, columnValue);
                                        orCombine5 = orCombine5 == null ? e1 : Expression.Or(orCombine5, e1);
                                    }
                                    combined = combined == null ? orCombine5 : Expression.And(combined, orCombine5);
                                    break;

                                case SearchCondition.LessThanEqual:
                                    Expression orCombine6 = null;
                                    foreach (var val in arrayValue)
                                    {
                                        var columnValue = val.GetType().FullName == "System.DateTime" ?
                                            Expression.Constant(DateTime.Parse(val.ToString()), typeof(DateTime)) : Expression.Constant(val);

                                        var e1 = Expression.LessThanOrEqual(columnNameProperty, columnValue);
                                        orCombine6 = orCombine6 == null ? e1 : Expression.Or(orCombine6, e1);
                                    }
                                    combined = combined == null ? orCombine6 : Expression.And(combined, orCombine6);
                                    break;

                                case SearchCondition.Contains:
                                    var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                                    Expression orCombine7 = null;
                                    foreach (var val in arrayValue)
                                    {
                                        var columnValue = Expression.Constant(val);
                                        dynamic e1 = Expression.Call(columnNameProperty, method, columnValue);
                                        orCombine7 = orCombine7 == null ? e1 : Expression.Or(orCombine7, e1);
                                    }

                                    combined = combined == null ? orCombine7 : Expression.And(combined, orCombine7);
                                    break;

                                case SearchCondition.DateTimeRange:
                                    var fromDate = Expression.Constant(DateTime.Parse(arrayValue[0].ToString()), typeof(DateTime));
                                    var toDate = arrayValue.Length < 1 ? null : Expression.Constant(DateTime.Parse(arrayValue[1].ToString()), typeof(DateTime));

                                    var fromExpression = Expression.GreaterThanOrEqual(columnNameProperty, fromDate);
                                    var toExpression = toDate == null ? null : Expression.LessThanOrEqual(columnNameProperty, toDate);
                                    var rangeExpression = toExpression == null ? fromExpression : Expression.And(fromExpression, toExpression);

                                    combined = combined == null ? rangeExpression : Expression.And(combined, rangeExpression);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }

                return Expression.Lambda<Func<T, bool>>(combined, pe);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SearchFieldList"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetWhereExpByType<T>(List<SearchDTO> SearchFieldList, Type item)
        {
            var pe = Expression.Parameter(item, "c");

            Expression combined = null;
            if (SearchFieldList != null)
            {
                foreach (var fieldItem in SearchFieldList)
                {
                    var columnNameProperty = Expression.Property(pe, fieldItem.SearchField);
                    var columnValue = Expression.Constant(fieldItem.SearchValue);
                    switch (fieldItem.SearchCondition)
                    {
                        case SearchCondition.Equal:
                            var e1 = Expression.Equal(columnNameProperty, columnValue);
                            combined = combined == null ? e1 : Expression.And(combined, e1);
                            break;

                        case SearchCondition.NotEqual:
                            var e2 = Expression.NotEqual(columnNameProperty, columnValue);
                            combined = combined == null ? e2 : Expression.And(combined, e2);
                            break;

                        default:
                            break;
                    }
                }
            }

            return Expression.Lambda<Func<T, bool>>(combined, pe);
        }

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