using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Infrastructure.Persistence;

public class DapperContext : IDapperContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);
}
