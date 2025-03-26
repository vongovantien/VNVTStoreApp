using FW.WAPI.Core.Configuration;
using FW.WAPI.Core.DAL.Model.Audit;
using FW.WAPI.Core.DAL.Model.Configuration;
using FW.WAPI.Core.DAL.Model.Email;
using FW.WAPI.Core.DAL.Model.Jwt;
using FW.WAPI.Core.DAL.Model.Notification;
using FW.WAPI.Core.DAL.Model.Tenant;
using FW.WAPI.Core.Domain.Context;
using FW.WAPI.Core.Domain.IntegrationEventLog;
using FW.WAPI.Core.Filter;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Infrastructure.Logger;
using FW.WAPI.Core.MultiTenancy;
using FW.WAPI.Core.MultiTenancy.Resolver;
using FW.WAPI.Core.Repository;
using FW.WAPI.Core.Runtime.Audit;
using FW.WAPI.Core.Runtime.Session;
using FW.WAPI.Core.Service;
using FW.WAPI.Core.Service.Audit;
using FW.WAPI.Core.Service.IntegrationEventLog;
using FW.WAPI.Core.Service.MultyTenancy;
using FW.WAPI.Core.Service.Notification;
using FW.WAPI.Core.Service.Warning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FW.WAPI.Core.Extension
{
    public static class CoreServiceExtension
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static void AddCoreService<HostDBContext, TenantDBContext, Tenant>
            (this IServiceCollection services, IConfiguration configuration, Action<CoreServiceOptions> options)
            where HostDBContext : DbContext
            where TenantDBContext : DbContext
            where Tenant : class, IBaseTenant
        {
            services.UseMinimalHttpLogger();
            var optCore = new CoreServiceOptions();
            options.Invoke(optCore);
            //Get connection string
            var connectionString = optCore.ConnectionType == EnumTypes.ConnectionType.AppConfig ?
                configuration.GetConnectionString(optCore.DefaultConnectionString) : optCore.DefaultConnectionString;
            //config compression
            services.AddResponseCompression();
            //config tenant data context
            services.AddDbContext<TenantDBContext>((provider, optionsDB) =>
            {
                var startupOption = provider.GetService<IStartupCoreOptions>();

                if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                {
                    if (string.IsNullOrEmpty(startupOption.ConnectionString))
                    {
                        optionsDB.UseSqlServer(connectionString);
                    }
                    else
                    {
                        optionsDB.UseSqlServer(startupOption.ConnectionString);
                    }
                }
                else if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                {
                    if (string.IsNullOrEmpty(startupOption.ConnectionString))
                    {
                        optionsDB.UseNpgsql(connectionString);
                    }
                    else
                    {
                        optionsDB.UseNpgsql(startupOption.ConnectionString);
                    }
                }
            });

            //Register DBcontext of event log
            if (optCore.isUseEventLog)
            {
                services.AddScoped(sp =>
                {
                    var hostContext = sp.GetRequiredService<TenantDBContext>();
                    var connection = hostContext.Database.GetDbConnection();

                    if (optCore.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                    {
                        var connectionBuilder = new DbContextOptionsBuilder<IntegrationEventLogContext>().UseSqlServer(connection);
                        var context = new IntegrationEventLogContext(connectionBuilder.Options, optCore.EventLogPKName,
                            optCore.EventLogTableName);
                        return context;
                    }
                    else if (optCore.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                    {
                        var connectionBuilder = new DbContextOptionsBuilder<IntegrationEventLogContext>().UseNpgsql(connection);
                        var context = new IntegrationEventLogContext(connectionBuilder.Options, optCore.EventLogPKName,
                            optCore.EventLogTableName);
                        return context;
                    }

                    return null;
                });

                services.AddScoped<IIntegrationEventLogService, IntegrationEventLogService>();
            }

            ////Config host data context
            services.AddDbContext<HostDBContext>((provider, optionsDB) =>
            {
                var startupOption = provider.GetService<IStartupCoreOptions>();

                if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                {
                    optionsDB.UseSqlServer(connectionString);
                }
                else if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                {
                    optionsDB.UseNpgsql(connectionString);
                }
            });

            services.AddScoped<IStartupCoreOptions>(s =>
            {
                var newOpt = new StartupCoreOptions(true, false, optCore.DatabaseProvider, optCore.SystemItemListType);
                newOpt.Tenant = typeof(Tenant);
                newOpt.EventLogTableName = optCore.isUseEventLog ? optCore.EventLogTableName : null;
                newOpt.EventLogPKName = optCore.isUseEventLog ? optCore.EventLogPKName : null;

                if (optCore.IsAuditLog)
                {
                    newOpt.IsAuditing = true;
                    newOpt.LogLevel = optCore.LogLevel;
                }

                return newOpt;
            });

            //Add exception filter
            services.AddMvc(cfg =>
            {
                if (optCore.IsAuditLog)
                {
                    cfg.Filters.Add(typeof(LogStashFilter));
                }

                cfg.Filters.Add(typeof(ExceptionFilter));
                cfg.Filters.Add(typeof(RoleAuthorizationFilter));
                cfg.Filters.Add(typeof(ResultFilter));

            }).AddJsonOptions(optionsJson =>
               {
                   optionsJson.SerializerSettings.ContractResolver
                       = new DefaultContractResolver();
                   optionsJson.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
               });

            ConfigureAspCoreServices<HostDBContext, Tenant>(services, configuration, optCore);

            services.AddTransient<IClientInfoProvider, HttpContextClientInfoProvider>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static void AddCoreService<HostDBContext, TenantDBContext, Tenant, TAudit>
            (this IServiceCollection services, IConfiguration configuration, Action<CoreServiceOptions> options)
            where HostDBContext : DbContext
            where TenantDBContext : DbContext
            where Tenant : class, IBaseTenant
            where TAudit : class
        {
            var optCore = new CoreServiceOptions();
            options.Invoke(optCore);

            var connectionString = optCore.ConnectionType == EnumTypes.ConnectionType.AppConfig ?
                configuration.GetConnectionString(optCore.DefaultConnectionString) : optCore.DefaultConnectionString;

            //config tenant data context
            services.AddDbContext<TenantDBContext>((provider, optionsDB) =>
            {
                var startupOption = provider.GetService<IStartupCoreOptions>();

                if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                {
                    if (string.IsNullOrEmpty(startupOption.ConnectionString))
                    {
                        optionsDB.UseSqlServer(connectionString);
                    }
                    else
                    {
                        optionsDB.UseSqlServer(startupOption.ConnectionString);
                    }
                }
                else if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                {
                    if (string.IsNullOrEmpty(startupOption.ConnectionString))
                    {
                        optionsDB.UseNpgsql(connectionString);
                    }
                    else
                    {
                        optionsDB.UseNpgsql(startupOption.ConnectionString);
                    }
                }
            });

            ////Config host data context
            services.AddDbContext<HostDBContext>((provider, optionsDB) =>
            {
                var startupOption = provider.GetService<IStartupCoreOptions>();

                if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                {
                    optionsDB.UseSqlServer(connectionString);
                }
                else if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                {
                    optionsDB.UseNpgsql(connectionString);
                }
            });

            if (optCore.isUseEventLog)
            {
                services.AddScoped(sp =>
                {
                    var hostContext = sp.GetRequiredService<TenantDBContext>();
                    var connection = hostContext.Database.GetDbConnection();

                    if (optCore.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                    {
                        var connectionBuilder = new DbContextOptionsBuilder<IntegrationEventLogContext>().UseSqlServer(connection);
                        var context = new IntegrationEventLogContext(connectionBuilder.Options, optCore.EventLogPKName,
                            optCore.EventLogTableName);
                        return context;
                    }
                    else if (optCore.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                    {
                        var connectionBuilder = new DbContextOptionsBuilder<IntegrationEventLogContext>().UseNpgsql(connection);
                        var context = new IntegrationEventLogContext(connectionBuilder.Options, optCore.EventLogPKName,
                            optCore.EventLogTableName);
                        return context;
                    }

                    return null;
                });

                services.AddScoped<IIntegrationEventLogService, IntegrationEventLogService>();
            }

            //services.AddScoped<IStartupCoreOptions>(s => new StartupCoreOptions(true, true, optCore.DatabaseProvider, optCore.SystemItemListType));
            services.AddScoped<IStartupCoreOptions>(s =>
            {
                var opt = new StartupCoreOptions(true, true, optCore.DatabaseProvider, optCore.SystemItemListType);
                opt.Tenant = typeof(Tenant);
                opt.EventLogTableName = optCore.isUseEventLog ? optCore.EventLogTableName : null;
                opt.EventLogPKName = optCore.isUseEventLog ? optCore.EventLogPKName : null;
                return opt;
            });
            //config compression
            services.AddResponseCompression();
            //config filter
            services.AddMvc(cfg =>
            {
                cfg.Filters.Add(typeof(ExceptionFilter));
                cfg.Filters.Add(typeof(AuditLogFilter));
                cfg.Filters.Add(typeof(TenantAuthorizationFilter));
                cfg.Filters.Add(typeof(RoleAuthorizationFilter));
                cfg.Filters.Add(typeof(ResultFilter));
            }
           ).AddJsonOptions(optionsJson =>
           {
               optionsJson.SerializerSettings.ContractResolver
                   = new DefaultContractResolver();
               optionsJson.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
           });

            //Add di to resolve audit
            services.AddTransient<IAuditLogResolver, AuditLogResolver<HostDBContext, TAudit>>();
            services.AddTransient<IAuditLogService<HostDBContext, TAudit>, AuditLogService<HostDBContext, TAudit>>();
            services.AddTransient<IClientInfoProvider, HttpContextClientInfoProvider>();

            ConfigureAspCoreServices<HostDBContext, Tenant>(services, configuration, optCore);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TenantDBContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="options"></param>
        public static void AddCoreService<TenantDBContext>(this IServiceCollection services, IConfiguration configuration, Action<CoreServiceOptions> options)
            where TenantDBContext : DbContext
        {
            var optCore = new CoreServiceOptions();
            options.Invoke(optCore);

            var connectionString = optCore.ConnectionType == EnumTypes.ConnectionType.AppConfig ?
                configuration.GetConnectionString(optCore.DefaultConnectionString) : optCore.DefaultConnectionString;
            ////Config data context
            services.AddDbContext<TenantDBContext>((provider, optionsDB) =>
            {
                var startupOption = provider.GetService<IStartupCoreOptions>();
                //optionsDB.UseSqlServer(connectionString);
                if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                {
                    optionsDB.UseSqlServer(connectionString);
                }
                else if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                {
                    optionsDB.UseNpgsql(connectionString);
                }
            });

            //Register DBcontext of event log
            if (optCore.isUseEventLog)
            {
                services.AddScoped(sp =>
                {
                    var hostContext = sp.GetRequiredService<TenantDBContext>();
                    var connection = hostContext.Database.GetDbConnection();

                    if (optCore.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                    {
                        var connectionBuilder = new DbContextOptionsBuilder<IntegrationEventLogContext>().UseSqlServer(connection);
                        var context = new IntegrationEventLogContext(connectionBuilder.Options, optCore.EventLogPKName,
                            optCore.EventLogTableName);
                        return context;
                    }
                    else if (optCore.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                    {
                        var connectionBuilder = new DbContextOptionsBuilder<IntegrationEventLogContext>().UseNpgsql(connection);
                        var context = new IntegrationEventLogContext(connectionBuilder.Options, optCore.EventLogPKName,
                            optCore.EventLogTableName);
                        return context;
                    }

                    return null;
                });
            }

            //config compression
            services.AddResponseCompression();
            //Add singleton to IStartupCoreOption
            services.AddScoped<IStartupCoreOptions>(s =>
            {
                var optC = new StartupCoreOptions(false, false, optCore.DatabaseProvider, optCore.SystemItemListType);
                optC.EventLogTableName = optCore.isUseEventLog ? optCore.EventLogTableName : null;
                optC.EventLogPKName = optCore.isUseEventLog ? optCore.EventLogPKName : null;
                if (optCore.IsAuditLog)
                {
                    optC.IsAuditing = true;
                    optC.LogLevel = optCore.LogLevel;
                }
                return optC;
            });

            services.AddTransient<IClientInfoProvider, HttpContextClientInfoProvider>();

            //Add exception filter
            services.AddMvc(cfg =>
            {
                cfg.Filters.Add(typeof(ExceptionFilter));
                cfg.Filters.Add(typeof(RoleAuthorizationFilter));
                cfg.Filters.Add(typeof(ResultFilter));
                if (optCore.IsAuditLog)
                {
                    cfg.Filters.Add(typeof(LogStashFilter));
                }
            }
           ).AddJsonOptions(optionsJson =>
           {
               optionsJson.SerializerSettings.ContractResolver
                   = new DefaultContractResolver();
               optionsJson.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
           });

            ConfigureAspCoreServices(services, configuration);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TenantDBContext"></typeparam>
        /// <typeparam name="TAudit"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="options"></param>
        public static void AddCoreService<TenantDBContext, TAudit>(this IServiceCollection services,
            IConfiguration configuration, Action<CoreServiceOptions> options)
            where TenantDBContext : DbContext
            where TAudit : class
        {
            var optCore = new CoreServiceOptions();
            options.Invoke(optCore);

            var connectionString = optCore.ConnectionType == EnumTypes.ConnectionType.AppConfig ?
                configuration.GetConnectionString(optCore.DefaultConnectionString) : optCore.DefaultConnectionString;
            ////Config data context
            services.AddDbContext<TenantDBContext>((provider, optionsDB) =>
            {
                var startupOption = provider.GetService<IStartupCoreOptions>();

                if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                {
                    optionsDB.UseSqlServer(connectionString);
                }
                else if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                {
                    optionsDB.UseNpgsql(connectionString);
                }
            });

            //Register DBcontext of event log
            //if (optCore.isUseEventLog)
            //{
            //    services.AddDbContext<IntegrationEventLogContext>((provider, optionsDB) =>
            //    {
            //        var startupOption = provider.GetService<IStartupCoreOptions>();

            //        if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
            //        {
            //            optionsDB.UseSqlServer(connectionString);
            //        }
            //        else if (startupOption.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
            //        {
            //            optionsDB.UseNpgsql(connectionString);
            //        }
            //    });
            //}

            //config compression
            services.AddResponseCompression();

            //Add singleton to IStartupCoreOption
            services.AddScoped<IStartupCoreOptions>(s => new StartupCoreOptions(false,
                true, optCore.DatabaseProvider, optCore.SystemItemListType));

            //config filter
            services.AddMvc(cfg =>
            {
                cfg.Filters.Add(typeof(ExceptionFilter));
                cfg.Filters.Add(typeof(AuditLogFilter));
                cfg.Filters.Add(typeof(RoleAuthorizationFilter));
                cfg.Filters.Add(typeof(ResultFilter));
            }
           ).AddJsonOptions(optionsJson =>
           {
               optionsJson.SerializerSettings.ContractResolver
                   = new DefaultContractResolver();
               optionsJson.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
           });

            services.AddTransient<IAuditLogResolver, AuditLogResolver<TenantDBContext, TAudit>>();
            services.AddTransient<IAuditLogService<TenantDBContext, TAudit>, AuditLogService<TenantDBContext, TAudit>>();
            services.AddTransient<IClientInfoProvider, HttpContextClientInfoProvider>();
            ConfigureAspCoreServices(services, configuration);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static void ConfigureAspCoreServices
            (IServiceCollection services, IConfiguration configuration, CoreServiceOptions coreServiceOptions = null)
        {
            //Config Jwt service
            //ConfigureJwtAuthService(services, configuration);

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //config CORS (Cross-Origin Resource Sharing)
            services.AddCors(options =>
            {
                if (coreServiceOptions != null && coreServiceOptions.AllowOrigins != null)
                {
                    options.AddPolicy(CorsPolicy.CORS_POLICY_NAME,
                  p => p.AllowAnyHeader().AllowAnyMethod().WithOrigins(coreServiceOptions.AllowOrigins).AllowCredentials());
                }
                else
                {
                    options.AddPolicy(CorsPolicy.CORS_POLICY_NAME,
                  p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials());
                }

            });

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory(CorsPolicy.CORS_POLICY_NAME));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    In = "header",
                    Description = "Please insert JWT with Bearer into field. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    Type = "apiKey"
                });
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });

            var config = new AutoMapper.MapperConfiguration(cfg =>
            {

            });

            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            services.Configure<Audience>(configuration.GetSection("Audience"));
            services.Configure<ResultCodeTable>(configuration.GetSection("ResultCode"));
            services.Configure<List<TableConfig>>(configuration.GetSection("TableSetting"));
            services.Configure<DistributedCompany>(configuration.GetSection("DistributedTable"));

            services.AddScoped<IDomainTenantResolve, DomainTenantResolveContributor>();
            services.AddScoped<IHttpCookieTenantResolve, HttpCookieTenantResolveContributor>();
            services.AddScoped<IHttpHeaderTenantResolve, HttpHeaderTenantResolveContributor>();

            services.AddScoped<ITenantResolver, TenantResolver>();
            services.AddScoped<IBaseSession, BaseSession>();
            //Service to create new database
            services.AddScoped<IContextManager, ContextManager>();
            services.AddScoped<IWebMultiTenancyConfiguration, WebMultiTenancyConfiguration>();

            services.AddScoped(typeof(IBaseService<,>), typeof(BaseService<,>));
            services.AddScoped(typeof(IRepository<,>), typeof(BaseRepository<,>));
            services.AddScoped<ICoreLogger, CoreLogger>();
            services.AddScoped<IWarningService, WarningService>();
            services.Configure<EmailConfig>(configuration.GetSection("EmailConfig"));
            services.Configure<NotificationConfig>(configuration.GetSection("NotificationConfig"));
            services.Configure<AuditLogConfig>(configuration.GetSection(nameof(AuditLogConfig)));

            services.AddScoped<IAuditLoggingService, AuditLoggingService>();
            services.AddHttpClient<IAuditLoggingService, AuditLoggingService>();

            services.AddScoped<INotifyService, NotifyService>();
            services.AddHttpClient<INotifyService, NotifyService>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static void ConfigureAspCoreServices<HostDBContext, Tenant>
            (IServiceCollection services, IConfiguration configuration, CoreServiceOptions optionsCore)
            where HostDBContext : DbContext
            where Tenant : class, IBaseTenant

        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //config CORS (Cross-Origin Resource Sharing)
            services.AddCors(options =>
            {
                if (optionsCore.AllowOrigins != null)
                {
                    options.AddPolicy(CorsPolicy.CORS_POLICY_NAME, p =>
                    p.AllowAnyHeader().AllowAnyMethod().WithOrigins(optionsCore.AllowOrigins).AllowCredentials());
                }
                else
                {
                    options.AddPolicy(CorsPolicy.CORS_POLICY_NAME, p =>
                        p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials());
                }

            });

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory(CorsPolicy.CORS_POLICY_NAME));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    In = "header",
                    Description = "Please insert JWT with Bearer into field. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    Type = "apiKey"
                });
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });

            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                //cfg.AddProfile(new AutoMapperProfileConfiguration());
            });

            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            services.Configure<Audience>(configuration.GetSection("Audience"));
            services.Configure<ResultCodeTable>(configuration.GetSection("ResultCode"));
            services.Configure<List<TableConfig>>(configuration.GetSection("TableSetting"));
            services.Configure<DistributedCompany>(configuration.GetSection("DistributedTable"));

            services.AddScoped<IDomainTenantResolve, DomainTenantResolveContributor>();
            services.AddScoped<IHttpCookieTenantResolve, HttpCookieTenantResolveContributor>();
            services.AddScoped<IHttpHeaderTenantResolve, HttpHeaderTenantResolveContributor>();
            services.AddScoped<ICoreLogger, CoreLogger>();

            services.AddScoped<ITenantResolver, TenantResolver>();
            //Service to create new database
            services.AddScoped<IContextManager, ContextManager>();
            services.AddScoped<IWebMultiTenancyConfiguration, WebMultiTenancyConfiguration>();

            services.AddScoped<ITenantCoreService<Tenant>, TenantCoreService<HostDBContext, Tenant>>();
            services.AddScoped<ITenantStore, TenantStore<Tenant>>();
            services.AddScoped<IBaseSession, BaseSession>();

            services.AddScoped(typeof(IBaseService<,>), typeof(BaseService<,>));
            services.AddScoped(typeof(IRepository<,>), typeof(BaseRepository<,>));

            services.AddScoped<IWarningService, WarningService>();
            services.Configure<EmailConfig>(configuration.GetSection("EmailConfig"));
            services.Configure<NotificationConfig>(configuration.GetSection("NotificationConfig"));

            services.Configure<AuditLogConfig>(configuration.GetSection(nameof(AuditLogConfig)));
            services.AddScoped<IAuditLoggingService, AuditLoggingService>();
            services.AddHttpClient<IAuditLoggingService, AuditLoggingService>();

            services.AddScoped<INotifyService, NotifyService>();
            services.AddHttpClient<INotifyService, NotifyService>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureJwtAuthService(this IServiceCollection services, IConfiguration configuration)
        {
            var audienceConfig = configuration.GetSection("Audience");
            var symmetricKeyAsBase64 = audienceConfig["Secret"];
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT issuer (Iss) claim
                ValidateIssuer = true,
                ValidIssuer = audienceConfig["Iss"],

                // Validate the JWT audience (Aud) claim
                ValidateAudience = true,
                ValidAudience = audienceConfig["Aud"],

                // Validate token expiration
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = tokenValidationParameters;
            });
        }
    }
}