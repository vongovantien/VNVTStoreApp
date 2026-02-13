using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using VNVTStore.Infrastructure.Services;

namespace VNVTStore.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Add JWT Authentication
        var secretKey = configuration["JwtSettings:SecretKey"] ?? "SuperSecretKeyForIntegrationTestingOnly1234567890!";
        var issuer = configuration["JwtSettings:Issuer"] ?? "VNVTStore";
        var audience = configuration["JwtSettings:Audience"] ?? "VNVTStoreUsers";
        var key = Encoding.ASCII.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Add Authorization
        services.AddAuthorization();

        // Add CORS
        // Add Cors
        var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowedOrigins", builder =>
            {
                builder.WithOrigins(allowedOrigins)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });

        // Add Rate Limiting
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        Status = "Error",
                        Message = $"Too many requests. Please try again after {retryAfter.TotalSeconds} second(s).",
                        RetryAfterSeconds = retryAfter.TotalSeconds
                    }, token);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        Status = "Error",
                        Message = "Too many requests. Please try again later."
                    }, token);
                }
            };

            // Global Fixed Window Limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                        ? httpContext.User.Identity.Name!
                        : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = configuration.GetValue<int>("RateLimiting:PermitLimit", 100),
                        QueueLimit = configuration.GetValue<int>("RateLimiting:QueueLimit", 0),
                        Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:WindowInSeconds", 60))
                    }));

            // Policy for Authentication (Sliding Window)
            options.AddSlidingWindowLimiter("AuthLimit", opt =>
            {
                opt.PermitLimit = configuration.GetValue<int>("RateLimiting:AuthPermitLimit", 5);
                opt.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:AuthWindowInSeconds", 10));
                opt.SegmentsPerWindow = 2;
                opt.QueueLimit = 0;
            });

            // Policy for Expensive Operations (Fixed Window)
            options.AddFixedWindowLimiter("ExpensiveLimit", opt =>
            {
                opt.PermitLimit = configuration.GetValue<int>("RateLimiting:ExpensivePermitLimit", 3);
                opt.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:ExpensiveWindowInSeconds", 30));
                opt.QueueLimit = 0;
            });
        });

        // Add Swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "VNVTStore API",
                Version = "v1",
                Description = "E-Commerce API built with Clean Architecture",
                Contact = new OpenApiContact
                {
                    Name = "VNVT",
                    Email = "vongovantien@gmail.com"
                }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Add Controllers
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });
        services.AddEndpointsApiExplorer();

        // Configure lowercase URLs
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
        });

        return services;
    }
}
