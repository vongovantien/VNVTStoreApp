using Moq;
using VNVTStore.Tests.Common;

namespace VNVTStore.Tests.Extensions;

public static class MockExtensions
{
    public static void ReturnsAsyncEnumerable<T>(this Microsoft.EntityFrameworkCore.DbSet<T> dbSet, IEnumerable<T> data)
        where T : class
    {
        var mockData = data.AsQueryable();
        // This extension is more for DbContext mocking directly, 
        // but since we use Repository pattern which returns IQueryable, 
        // we might need a different approach or use this for whatever returns IQueryable
    }

    public static IQueryable<T> BuildMock<T>(this IEnumerable<T> data)
    {
        return new TestAsyncEnumerable<T>(data);
    }
}
