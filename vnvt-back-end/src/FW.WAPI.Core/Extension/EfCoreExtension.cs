using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;


namespace FW.WAPI.Core.Extension
{
    public static class EfCoreExtension
    {
        /// <summary>
        /// Find primary key of entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <seealso cref="https://stackoverflow.com/questions/30688909/how-to-get-primary-key-value-with-entity-framework-core"/>
        /// <param name="dbContext"></param>
        /// <returns>enumrable string primary key or null</returns>
        public static IEnumerable<string> FindPrimaryKeyNames<T>(this DbContext dbContext)
        {
            var properties = dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties;
            return properties != null ? properties.Select(x => x.Name) : null;
        }
    }
}
