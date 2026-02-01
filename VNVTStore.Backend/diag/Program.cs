using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Dapper;

string connectionString = "Host=localhost;Database=shoppingdb;Username=postgres;Password=password";

Console.WriteLine("Starting diagnostics...");
using var connection = new NpgsqlConnection(connectionString);
try 
{
    await connection.OpenAsync();
    Console.WriteLine("Connection opened.");

    var tables = await connection.QueryAsync<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'");
    Console.WriteLine("Tables: " + string.Join(", ", tables));

    var filesCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM \"TblFile\"");
    Console.WriteLine($"Total files in TblFile: {filesCount}");

    if (filesCount > 0)
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
    
    // Check if there are any files with MasterType = 'Product'
    var prodFilesCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM \"TblFile\" WHERE \"MasterType\" = 'Product'");
    Console.WriteLine($"Total product images (MasterType=Product): {prodFilesCount}");

    if (prodFilesCount > 0) {
        var sampleProductCodes = await connection.QueryAsync<string>("SELECT \"MasterCode\" FROM \"TblFile\" WHERE \"MasterType\" = 'Product' LIMIT 5");
        Console.WriteLine($"Sample MasterCodes in TblFile: {string.Join(", ", sampleProductCodes)}");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
    if (ex.InnerException != null) Console.WriteLine("Inner: " + ex.InnerException.Message);
}
