using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Products.Handlers;
using VNVTStore.Application.Products.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Tests.Products;

public class ProductHandlersTests
{
    private readonly Mock<IRepository<TblProduct>> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProductHandlers _handler;

    public ProductHandlersTests()
    {
        _repoMock = new Mock<IRepository<TblProduct>>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _handler = new ProductHandlers(_repoMock.Object, _uowMock.Object, _mapperMock.Object);
    }
    
    private TblProduct CreateTestProduct(string code, string name, string sku, decimal price, string? categoryCode = null)
    {
        var p = new TblProduct 
        { 
            Code = code, 
            Name = name, 
            Sku = sku, 
            Price = price, 
            CategoryCode = categoryCode, 
            IsActive = true 
        };
        return p;
    }

    [Fact]
    public async Task CreateProduct_ValidData_ReturnsSuccess()
    {
        // Arrange
        var requestDto = new CreateProductDto { Name = "Laptop", Price = 1000, Sku = "SKU123" };
        var productDto = new ProductDto { Name = "Laptop", Code = "P123456" };

        _repoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TblProduct, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblProduct?)null); 
            
        _repoMock.Setup(r => r.AddAsync(It.IsAny<TblProduct>(), It.IsAny<CancellationToken>()))
            .Callback<TblProduct, CancellationToken>((p, c) => {
                Assert.True(p.IsActive);
                Assert.NotNull(p.Code);
                Assert.StartsWith("P", p.Code);
            })
            .Returns(Task.CompletedTask);

        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        // Fix: Mock Input Mapping
        _mapperMock.Setup(m => m.Map<TblProduct>(It.IsAny<CreateProductDto>()))
            .Returns((CreateProductDto d) => new TblProduct { Name = d.Name, Price = d.Price, Sku = d.Sku });
        _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<TblProduct>())).Returns(productDto);

        var cmd = new CreateCommand<CreateProductDto, ProductDto>(requestDto);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<TblProduct>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_ReturnsConflict()
    {
        // Arrange
        var requestDto = new CreateProductDto { Name = "Laptop", Price = 1000, Sku = "EXISTING_SKU" };
        var existingProduct = CreateTestProduct("P001", "Old Laptop", "EXISTING_SKU", 900);

        _repoMock.Setup(r => r.FindAsync(
            It.IsAny<Expression<Func<TblProduct, bool>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        var cmd = new CreateCommand<CreateProductDto, ProductDto>(requestDto);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Conflict", result.Error!.Code);
    }

    [Fact]
    public async Task UpdateProduct_ChangeToExistingSku_ReturnsConflict()
    {
        // Arrange
        var code = "P001";
        var requestDto = new UpdateProductDto { Name = "Updated", Sku = "OTHER_SKU" };
        var currentProduct = CreateTestProduct(code, "Old Name", "MY_SKU", 100);
        var otherProduct = CreateTestProduct("P002", "Other", "OTHER_SKU", 200);

        _repoMock.Setup(r => r.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentProduct);

        _repoMock.Setup(r => r.FindAsync(
            It.IsAny<Expression<Func<TblProduct, bool>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherProduct);

        var cmd = new UpdateCommand<UpdateProductDto, ProductDto>(code, requestDto);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Conflict", result.Error!.Code);
    }
}
