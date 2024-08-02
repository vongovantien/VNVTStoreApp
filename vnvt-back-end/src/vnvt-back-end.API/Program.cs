
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.MappingProfiles;
using vnvt_back_end.Application.Services;
using vnvt_back_end.Infrastructure;
using vnvt_back_end.Infrastructure.Contexts;
using vnvt_back_end.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.   
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers()
    .AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce.API", Version = "v1" });
});

//Add AutoMapper
builder.Services.AddAutoMapper(typeof(ProductProfile));

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Configure lowercase URLs
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true; // Optional, if you also want query strings to be lowercase
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();
app.Run();
