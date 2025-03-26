using FW.WAPI.Core.Attribute;
using FW.WAPI.Core.ExceptionHandling;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FW.WAPI.Core.General
{
    public static class PropertyUtilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> CheckValidateFields(DbContext dbContext, List<string> fields, Type type)
        {
            var rootFields = fields.Where(x => !x.Contains(".")).ToList();
            var columnNames = Utilities.GetColumnNames(dbContext, type);

            var parentColumns = type.GetProperties().
                Where(x => x.GetCustomAttribute(typeof(ParentAttribute)) != null);

            var oneToOneAttribute = type.GetProperties().
               Where(x => x.GetCustomAttribute(typeof(OneToOneAttribute)) != null);

            var oneToOneAttributeNames = oneToOneAttribute != null ? oneToOneAttribute.Select(x => x.Name) : new List<string>();
            var parentColumnNames = parentColumns != null ? parentColumns.Select(x => x.Name).ToList() : new List<string>();
            if (columnNames.Any())
            {
                columnNames.AddRange(parentColumnNames);

                var matchCols = columnNames.Where(x => rootFields.FindIndex(p => p.Equals(x, StringComparison.OrdinalIgnoreCase)) > -1);

                foreach (var item in matchCols)
                {
                    var index = rootFields.FindIndex(x => x.Equals(item, StringComparison.OrdinalIgnoreCase));
                    if (index > -1)
                    {
                        rootFields[index] = item;
                    }
                }

                //columnNames.AddRange(oneToOneAttributeNames);
                var colNotInclude = rootFields.Where(x => !columnNames.Contains(x) || oneToOneAttributeNames.Contains(x));
                rootFields.RemoveAll(x => colNotInclude.Contains(x));
            }

            return rootFields;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static dynamic GetPropValueOfObject(object source, string propName)
        {
            var field = source.GetType().GetProperty(propName);

            if (field == null)
            {
                var fieldCheck = source.GetType().GetField(propName);

                if (fieldCheck == null)
                {
                    throw new PropertyNotExistException();
                }
                else
                {
                    return fieldCheck.GetValue(source);
                }
            }

            var val = field.GetValue(source);
            return val;
        }

        /// <summary>
        /// If field not in entity column or parent columns will be remove
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static List<string> CheckValidateFields<TEntity>(DbContext dbContext, List<string> fields) where TEntity : class
        {
            var rootFields = fields.Where(x => !x.Contains(".")).ToList();
            var columnNames = Utilities.GetColumnNames<TEntity>(dbContext);
            var parentColumns = typeof(TEntity).GetProperties().
                Where(x => x.GetCustomAttribute(typeof(ParentAttribute)) != null);

            var oneToOneAttribute = typeof(TEntity).GetProperties().
               Where(x => x.GetCustomAttribute(typeof(OneToOneAttribute)) != null);
            var sumAttribute = typeof(TEntity).GetProperties().
             Where(x => x.GetCustomAttribute(typeof(OneToOneAttribute)) != null);

            var oneToOneAttributeNames = oneToOneAttribute != null ? oneToOneAttribute.Select(x => x.Name) : new List<string>();
            var parentColumnNames = parentColumns != null ? parentColumns.Select(x => x.Name).ToList() : new List<string>();
            var sumColNames = sumAttribute != null ? sumAttribute.Select(x => x.Name) : new List<string>();
            if (columnNames.Any())
            {
                columnNames.AddRange(parentColumnNames);
                //columnNames.AddRange(oneToOneAttributeNames);
                var colNotInclude = rootFields.Where(x => !columnNames.Contains(x) || 
                    oneToOneAttributeNames.Contains(x) || sumColNames.Contains(x));
                rootFields.RemoveAll(x => colNotInclude.Contains(x));
            }

            return rootFields;
        }
    }
}
