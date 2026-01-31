using VNVTStore.API.Extensions;
using VNVTStore.API.Middlewares;
using VNVTStore.Application;
using VNVTStore.Infrastructure;
using Scalar.AspNetCore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using VNVTStore.Application.Seeding;
using VNVTStore.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/vnvtstore-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting VNVTStore API...");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddSignalR();
    builder.Services.AddHttpContextAccessor();

// Initialize Firebase Admin SDK
var firebaseKeyPath = Path.Combine(builder.Environment.ContentRootPath, "firebase-service-account.json");
if (File.Exists(firebaseKeyPath))
{
/*
    try 
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            var jsonText = File.ReadAllText(firebaseKeyPath);
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(jsonText)
            });
            Console.WriteLine("[Info] Firebase Admin SDK initialized successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Error] Firebase Init Failed: {ex.Message}");
    }
*/
    Console.WriteLine("[Warning] Firebase initialization skipped for debugging/migrations.");
}
else
{
    Console.WriteLine("[Warning] Firebase service account file not found.");
}

// Increase Request Body Limits for Large Image Uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200 * 1024 * 1024; // 200MB
    options.ValueLengthLimit = 200 * 1024 * 1024; // 200MB just in case
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 200 * 1024 * 1024; // 200MB
});

var app = builder.Build();

// Seed RBAC Permissions
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    
    // Automatically apply migrations
    if (context is DbContext dbContext)
    {
        await dbContext.Database.MigrateAsync();
    }
    
    await PermissionSeeder.SeedAsync(context);
    await DataSeeder.SeedAsync(context, passwordHasher);
}

// Configure the HTTP request pipeline
// Enable Swagger/Scalar in all environments for demo purposes
app.UseSwagger(options =>
{
    options.RouteTemplate = "openapi/{documentName}.json";
});
app.MapScalarApiReference(options =>
{
    options.WithTitle("VNVTStore API")
           .WithTheme(Scalar.AspNetCore.ScalarTheme.DeepSpace)
           .WithDefaultHttpClient(Scalar.AspNetCore.ScalarTarget.CSharp, Scalar.AspNetCore.ScalarClient.HttpClient);
});

if (app.Environment.IsDevelopment())
{
    // Dev specific middleware if any
}

app.UseHttpsRedirection();

app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors("AllowedOrigins");

app.UseStaticFiles();

app.UseRateLimiter();

// Add Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

// Add request localization middleware
app.UseLanguageLocalization();

app.UseExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<VNVTStore.Infrastructure.Hubs.NotificationHub>("/notificationHub");

app.MapGet("/", () => "VNVTStore API is running! Access docs at /scalar/v1");

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
