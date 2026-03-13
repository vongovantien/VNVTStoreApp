using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Persistence.Seeders;

public class ShopConfigSeeder : IDataSeeder
{
    private readonly IRepository<TblSystemConfig> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ShopConfigSeeder(IRepository<TblSystemConfig> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public int Order => 10;

    public async Task SeedAsync()
    {
        var configs = new List<TblSystemConfig>
        {
            new TblSystemConfig { Code = "MAINTENANCE_MODE", ConfigValue = "false", Description = "Global maintenance mode toggle" },
            new TblSystemConfig { Code = "SHOP_THEME", ConfigValue = "modern", Description = "Active shop theme (classic, modern, cyberpunk)" },
            new TblSystemConfig { Code = "LOYALTY_POINT_VALUE", ConfigValue = "10000", Description = "VND spend required for 1 loyalty point" },
            new TblSystemConfig { Code = "VIP_THRESHOLD", ConfigValue = "5000", Description = "Points required for VIP tier" },
            new TblSystemConfig { Code = "LOYAL_THRESHOLD", ConfigValue = "1000", Description = "Points required for Loyal tier" },
            new TblSystemConfig { Code = "SHOP_LOGO_URL", ConfigValue = "/logo.png", Description = "Shop global logo URL" },
            new TblSystemConfig { Code = "ANNOUNCEMENT_BANNER", ConfigValue = "Welcome to VNVT Store!", Description = "Text for the top announcement banner" }
        };

        foreach (var config in configs)
        {
            var existing = await _repository.GetByCodeAsync(config.Code, default);
            if (existing == null)
            {
                await _repository.AddAsync(config, default);
            }
        }

        await _unitOfWork.CommitAsync();
    }
}
