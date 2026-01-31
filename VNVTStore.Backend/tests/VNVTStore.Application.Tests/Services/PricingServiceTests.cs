using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Infrastructure.Persistence.Repositories;
using VNVTStore.Infrastructure.Services;
using Xunit;
using Moq;

namespace VNVTStore.Application.Tests.Services;

public class PricingServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly PricingService _pricingService;
    private readonly Mock<ICacheService> _cacheServiceMock;

    public PricingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _cacheServiceMock = new Mock<ICacheService>();

        var productRepo = new Repository<TblProduct>(_context);
        var productUnitRepo = new Repository<TblProductUnit>(_context);
        var userRepo = new Repository<TblUser>(_context);

        _pricingService = new PricingService(productRepo, productUnitRepo, userRepo, _cacheServiceMock.Object);
        
        SeedData();
    }

    private void SeedData()
    {
        // wholesalePrice passed as 90000 in constructor
        var product = TblProduct.Create("Product Name", 100000, 90000, 100, "CAT1", 10, "SUP1", "BRAND1");
        product.Code = "CABLE-100"; // EXPLICITLY SET CODE for test lookup
        // product.WholesalePrice = 90000; // Handled in Create
        
        var unit = new TblUnit { Code = "BOX", Name = "Box" };
        var activeUnit = new TblProductUnit 
        { 
            Code = "PU1",
            ProductCode = product.Code, 
            UnitCode = "BOX",
            ConversionRate = 10,  // 1 Box = 10 Cables
            Price = 950000,       // Bulk Box Price (slightly cheaper than 10 * 100,000)
            IsActive = true
        };

        // Create Users with correct Enum and arguments
        // Create(username, email, passwordHash, fullName, role)
        var userRetail = TblUser.Create("retail_user", "retail@test.com", "pass", "Retail", UserRole.Customer);
        userRetail.Code = "retail_user"; // EXPLICITLY SET CODE

        var userContractor = TblUser.Create("contractor_user", "contractor@test.com", "pass", "Contractor", UserRole.Customer);
        userContractor.Code = "contractor_user"; // EXPLICITLY SET CODE
        userContractor.UpdateRole("CONTRACTOR"); // Update RoleCode explicitly

        _context.TblProducts.Add(product);
        _context.TblUnits.Add(unit);

        _context.TblProductUnits.Add(activeUnit);
        _context.TblUsers.AddRange(userRetail, userContractor);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CalculatePrice_ShouldReturnBasePrice_ForRetailUser_NoUnit()
    {
        // Act
        var price = await _pricingService.CalculatePriceAsync("CABLE-100", null, "retail_user");

        // Assert
        price.Should().Be(100000);
    }

    [Fact]
    public async Task CalculatePrice_ShouldReturnWholesalePrice_ForContractor_NoUnit()
    {
        // Act
        var price = await _pricingService.CalculatePriceAsync("CABLE-100", null, "contractor_user");

        // Assert
        price.Should().Be(90000);
    }

    [Fact]
    public async Task CalculatePrice_ShouldReturnUnitOverridePrice_WhenUnitHasPrice()
    {
        // Act
        // Requesting a "BOX" (UnitCode=BOX). Configured Price is 950,000
        var price = await _pricingService.CalculatePriceAsync("CABLE-100", "BOX", "retail_user");

        // Assert
        price.Should().Be(950000);
    }

    [Fact]
    public async Task CalculatePrice_ShouldUseConversionRate_WhenUnitHasNoPrice()
    {
        // Arrange: Create a new unit "METER" with no explicit price, only conversion rate (0.5)
        var unit = new TblUnit { Code = "METER", Name = "Meter" };
        var pUnit = new TblProductUnit 
        { 
            Code = "PU2",
            ProductCode = "CABLE-100", 
            UnitCode = "METER",
            ConversionRate = 0.5m, 
            Price = 0, // No override price
            IsActive = true
        };
        _context.TblUnits.Add(unit);
        _context.TblProductUnits.Add(pUnit);
        _context.SaveChanges();

        // Act
        // Base Price 100,000 * 0.5 = 50,000
        var price = await _pricingService.CalculatePriceAsync("CABLE-100", "METER", "retail_user");

        // Assert
        price.Should().Be(50000);
    }
}
