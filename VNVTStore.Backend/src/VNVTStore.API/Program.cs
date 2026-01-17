using VNVTStore.API.Extensions;
using VNVTStore.API.Middlewares;
using VNVTStore.Application;
using VNVTStore.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddSignalR();

var app = builder.Build();

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

// Add request localization middleware
app.UseLanguageLocalization();

app.UseExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<VNVTStore.Infrastructure.Hubs.NotificationHub>("/notificationHub");

app.MapGet("/", () => "VNVTStore API is running! Access docs at /scalar/v1");

app.Run();
