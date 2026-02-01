
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VNVTStore.Application.Interfaces;
using VNVTStore.Infrastructure.Persistence;
using Dapper;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IDapperContext, DapperContext>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Add minimal infrastructure for DB connection
var connectionString = "Host=localhost;Database=shoppingdb;Username=postgres;Password=password";

using var host = builder.Build();
var dapperContext = host.Services.GetRequiredService<IDapperContext>();

using var connection = dapperContext.CreateConnection();
Console.WriteLine("Testing connection...");
try 
{
    var tables = await connection.QueryAsync<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'");
    Console.WriteLine("Tables: " + string.Join(", ", tables));

    var filescount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM \"TblFile\"");
    Console.WriteLine($"Total files in TblFile: {filescount}");

    if (filescount > 0)
    {
        var sampleFiles = await connection.QueryAsync<dynamic>("SELECT * FROM \"TblFile\" LIMIT 5");
        foreach (var f in sampleFiles)
        {
            var dict = (IDictionary<string, object>)f;
            Console.WriteLine("File Data: " + string.Join(", ", dict.Select(kv => $"{kv.Key}={kv.Value}")));
        }
    }

    var productsCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM \"TblProduct\"");
    Console.WriteLine($"Total products: {productsCount}");
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
}
