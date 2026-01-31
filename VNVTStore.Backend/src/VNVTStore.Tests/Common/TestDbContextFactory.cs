using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Infrastructure.Persistence;

namespace VNVTStore.Tests.Common;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
