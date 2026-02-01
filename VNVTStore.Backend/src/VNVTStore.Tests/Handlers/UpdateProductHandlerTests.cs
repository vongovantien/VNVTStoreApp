using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Products.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Tests.Common;
using Xunit;

namespace VNVTStore.Tests.Handlers;

public class UpdateProductHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IBaseUrlService> _mockBaseUrlService;
    private readonly Mock<IDapperContext> _mockDapperContext;
    private readonly Mock<IRepository<TblProduct>> _mockRepository;
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<TblProduct>()))
            .Returns((TblProduct source) => new ProductDto 
            { 
                Code = source.Code, 
                Name = source.Name,
                // Initialize collections to avoid null issues in other parts if accessed
                ProductImages = new List<ProductImageDto>(),
                Details = new List<ProductDetailDto>(),
                ProductUnits = new List<UnitDto>(),
                Variants = new List<ProductVariantDto>()
            });

        _mockFileService = new Mock<IFileService>();
        _mockBaseUrlService = new Mock<IBaseUrlService>();
        _mockBaseUrlService.Setup(x => x.GetBaseUrl()).Returns("http://test-api.com");
        _mockDapperContext = new Mock<IDapperContext>();
        
        // We use the real DbContext for the repository part in a way or mock it
        // Actually, BaseHandler uses IRepository<T>. Let's mock it but point to the context if possible
        // Or just mock the repository to return the context's DbSet
        _mockRepository = new Mock<IRepository<TblProduct>>();
        _mockRepository.Setup(r => r.AsQueryable()).Returns(_context.TblProducts);
        _mockRepository.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((code, ct) => 
                _context.TblProducts.FirstOrDefaultAsync(p => p.Code == code, ct));

        _handler = new UpdateProductHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockDapperContext.Object,
            _mockBaseUrlService.Object,
            _mockFileService.Object,
            _context
        );
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_UpdateProductWithChildren_ShouldReplaceChildren()
    {
        // Seed dependencies to avoid Foreign Key violations
        await _context.TblCategories.AddAsync(new TblCategory { Code = "CAT01", Name = "Category 1", IsActive = true });
        await _context.TblBrands.AddAsync(new TblBrand { Code = "BRAND01", Name = "Brand 1", IsActive = true });
        await _context.TblUnits.AddAsync(new TblUnit { Code = "UNIT1", Name = "Cái", IsActive = true });
        await _context.TblSuppliers.AddAsync(new TblSupplier { Code = "SUP01", Name = "Supplier 1", IsActive = true });
        
        var product = TblProduct.Create("Old Name", 1000, 800, 10, "CAT01", 500, "SUP01", "BRAND01", "Cái");
        product.Code = "PROD001";
        
        // Add initial children
        product.TblProductDetails.Add(new TblProductDetail { Code = "D1", SpecName = "Old Spec", SpecValue = "Old Value", IsActive = true, ProductCode = product.Code });
        product.TblProductUnits.Add(new TblProductUnit { Code = "U1", UnitCode = "UNIT1", Price = 1200, ConversionRate = 12, IsActive = true, ProductCode = product.Code });
        
        await _context.TblProducts.AddAsync(product);
        await _context.TblUnits.AddAsync(new TblUnit { Code = "UNIT_NEW", Name = "Thùng", IsActive = true });
        await _context.SaveChangesAsync();

        var updateDto = new UpdateProductDto
        {
            Name = "New Name",
            Details = new List<CreateProductDetailDto>
            {
                new CreateProductDetailDto { SpecName = "New Spec", SpecValue = "New Value", DetailType = ProductDetailType.SPEC }
            },
            ProductUnits = new List<CreateUnitDto>
            {
                new CreateUnitDto { UnitName = "Thùng", Price = 500, ConversionRate = 24, IsBaseUnit = false }
            },
            Variants = new List<CreateProductVariantDto>
            {
                new CreateProductVariantDto { SKU = "SKU-001", Price = 1000, StockQuantity = 10, Attributes = "Color: Red" }
            }
        };

        var request = new UpdateCommand<UpdateProductDto, ProductDto>(product.Code, updateDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(result.Error?.Message ?? result.Error?.Code);
        
        // Reload product with children
        var updatedProduct = await _context.TblProducts
            .Include(p => p.TblProductDetails)
            .Include(p => p.TblProductUnits)
            .Include(p => p.TblProductVariants)
            .FirstOrDefaultAsync(p => p.Code == product.Code);

        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be("New Name");
        
        // Verify Details were replaced
        updatedProduct.TblProductDetails.Should().HaveCount(1);
        updatedProduct.TblProductDetails.First().SpecName.Should().Be("New Spec");
        
        // Verify Units were replaced
        updatedProduct.TblProductUnits.Should().HaveCount(1);
        updatedProduct.TblProductUnits.First().Price.Should().Be(500);

        // Verify Variants were added
        updatedProduct.TblProductVariants.Should().HaveCount(1);
        updatedProduct.TblProductVariants.First().SKU.Should().Be("SKU-001");

        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateProductWithImages_ShouldCallSyncProductImages()
    {
        // Arrange
        var product = TblProduct.Create("Product With Images", 1000, 0, 10, "CAT01", 0, null, null, "Cái");
        product.Code = "PROD_IMG";
        await _context.TblProducts.AddAsync(product);
        await _context.SaveChangesAsync();

        var images = new List<string> { "data:image/png;base64,abc", "https://res.cloudinary.com/demo/image/upload/v1/sample.jpg" };
        var updateDto = new UpdateProductDto
        {
            Images = images
        };

        _mockFileService.Setup(f => f.SyncProductImagesAsync(product.Code, images, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var request = new UpdateCommand<UpdateProductDto, ProductDto>(product.Code, updateDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockFileService.Verify(f => f.SyncProductImagesAsync(product.Code, images, It.IsAny<CancellationToken>()), Times.Once);
    }
}
