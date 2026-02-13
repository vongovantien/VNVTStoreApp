using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Common;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Infrastructure.Persistence.Repositories;
using VNVTStore.Infrastructure.Services;
using Xunit;

namespace VNVTStore.Application.Tests.Complex;

public class ComplexPricingAndDebtTests
{
    private readonly ApplicationDbContext _context;
    private readonly PricingService _pricingService;
    private readonly DebtService _debtService;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;

    public ComplexPricingAndDebtTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _cacheServiceMock = new Mock<ICacheService>();
        _currentUserMock = new Mock<ICurrentUser>();

        var productRepo = new Repository<TblProduct>(_context);
        var productUnitRepo = new Repository<TblProductUnit>(_context);
        var userRepo = new Repository<TblUser>(_context);
        var debtLogRepo = new Repository<TblDebtLog>(_context);
        var unitOfWork = new UnitOfWork(_context);

        _pricingService = new PricingService(productRepo, productUnitRepo, userRepo, _cacheServiceMock.Object);
        _debtService = new DebtService(userRepo, debtLogRepo, unitOfWork, _currentUserMock.Object);
        
        SeedComplexData();
    }

    private void SeedComplexData()
    {
        // 1. Setup Product with complex pricing
        var product = TblProduct.Create("COMPLEX-PROD", 100000, 80000, 1000, "CAT-COMPLEX", 50000, "SUP-1");
        product.Code = "CP001";
        
        // 2. Setup Units
        var unitBox = new TblUnit { Code = "BOX", Name = "Box" };
        var boxPricing = new TblProductUnit 
        { 
            Code = "CP001_BOX",
            ProductCode = "CP001",
            UnitCode = "BOX",
            ConversionRate = 10,
            Price = 750000, // Box price is cheaper than 10 singles
            IsActive = true
        };

        // 3. Setup Users
        var retailUser = TblUser.Create("retailer", "retailer@test.com", "hash", "Retailer", UserRole.Customer);
        retailUser.Code = "USR_RETAIL";
        retailUser.UpdateDebtLimit(1000000);

        var contractorUser = TblUser.Create("contractor", "contractor@test.com", "hash", "Contractor", UserRole.Customer);
        contractorUser.Code = "USR_CONT";
        contractorUser.UpdateRole("CONTRACTOR");
        contractorUser.UpdateDebtLimit(5000000);

        _context.TblProducts.Add(product);
        _context.TblUnits.Add(unitBox);
        _context.TblProductUnits.Add(boxPricing);
        _context.TblUsers.AddRange(retailUser, contractorUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CalculatePrice_Complex_BoxPricing_ShouldOverrideWholesale()
    {
        // Contractor gets 80,000 normally. But if they buy a BOX, it should be 750,000 (75,000/ea).
        var price = await _pricingService.CalculatePriceAsync("CP001", "BOX", "USR_CONT");
        price.Should().Be(750000);
    }

    [Fact]
    public async Task DebtService_RecordChange_ShouldUpdateBalanceAndLog()
    {
        // USR_RETAIL starts with 0 debt.
        await _debtService.RecordDebtChangeAsync("USR_RETAIL", 100000, "Order #1");
        
        var balance = await _debtService.GetCurrentBalanceAsync("USR_RETAIL");
        balance.Should().Be(100000);

        var logs = await _context.TblDebtLogs.Where(l => l.UserCode == "USR_RETAIL").ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].Amount.Should().Be(100000);
    }

    [Fact]
    public async Task DebtService_CheckLimit_ShouldFail_WhenExceeded()
    {
        // Limit is 1,000,000
        var result = await _debtService.CheckDebtLimitAsync("USR_RETAIL", 1500000);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("DebtLimitExceeded");
    }
}
