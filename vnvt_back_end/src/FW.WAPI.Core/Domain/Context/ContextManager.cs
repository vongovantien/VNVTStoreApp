using FW.WAPI.Core.Configuration;
using FW.WAPI.Core.ExceptionHandling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Domain.Context
{
    public class ContextManager : IContextManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IStartupCoreOptions _startupCoreOptions;

        public ContextManager(IServiceProvider serviceProvider, IStartupCoreOptions startupCoreOptions)
        {
            _serviceProvider = serviceProvider;
            _startupCoreOptions = startupCoreOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<bool> CreateTenantDb<TenantContext>(string connectionString) where TenantContext : DbContext
        {
            var result = true;
            var options = new DbContextOptionsBuilder<TenantContext>();
            switch (_startupCoreOptions.DatabaseProvider)
            {
                case General.EnumTypes.DatabaseProvider.MSSQL:
                    options.UseSqlServer(connectionString);
                    break;
                case General.EnumTypes.DatabaseProvider.POSTGRESQL:
                    options.UseNpgsql(connectionString);
                    break;
                default:
                    break;
            }

            using (TenantContext context = (TenantContext)ActivatorUtilities.CreateInstance(_serviceProvider,
                typeof(TenantContext), new object[] { options.Options }))
            {
                //Check database is exist or not
                if (context.Database.GetService<IRelationalDatabaseCreator>().Exists())
                {
                    throw new TenantDBExistException("Database is exist");
                }
                else
                {
                    //Create database
                    await context.Database.MigrateAsync();

                    await context.SaveChangesAsync();
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TenantContext"></typeparam>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public TenantContext GetTenantContext<TenantContext>(string connectionString) where TenantContext : DbContext
        {
            var options = new DbContextOptionsBuilder<TenantContext>();
            switch (_startupCoreOptions.DatabaseProvider)
            {
                case General.EnumTypes.DatabaseProvider.MSSQL:
                    options.UseSqlServer(connectionString);
                    break;
                case General.EnumTypes.DatabaseProvider.POSTGRESQL:
                    options.UseNpgsql(connectionString);
                    break;
                default:
                    break;
            }

            return (TenantContext)ActivatorUtilities.CreateInstance(_serviceProvider,
                typeof(TenantContext), new object[] { options.Options });
        }
    }
}
