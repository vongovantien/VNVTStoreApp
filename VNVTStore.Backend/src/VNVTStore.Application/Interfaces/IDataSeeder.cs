using VNVTStore.Application.Common;

namespace VNVTStore.Application.Interfaces;

public interface IDataSeeder
{
    Task SeedAsync();
}
