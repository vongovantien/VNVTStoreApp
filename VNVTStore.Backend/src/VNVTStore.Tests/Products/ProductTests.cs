
using Moq;
using VNVTStore.Application.Products.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using AutoMapper;
using VNVTStore.Application.DTOs;
using System.Linq.Expressions;
using VNVTStore.Application.Products.Handlers;

namespace VNVTStore.Tests.Products;

public class ProductTests
{
    private readonly Mock<IRepository<TblProduct>> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public ProductTests()
    {
        _productRepositoryMock = new Mock<IRepository<TblProduct>>();
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task GetProduct_ExistingId_ReturnsProduct()
    {
        // Arrange
        var productCode = "P001";
        var product = TblProduct.Create("Test Product", 100, 10, null, "SKU123");
        typeof(TblProduct).GetProperty("Code")!.SetValue(product, productCode);

        var query = new GetProductByCodeQuery(productCode);
        var dto = new ProductDto { Code = productCode, Name = "Test Product" };

        // Mock AsQueryable because Handler likely uses it with Includes
        var list = new List<TblProduct> { product };
        var mockSet = new VNVTStore.Tests.Common.TestAsyncEnumerable<TblProduct>(list);
        _productRepositoryMock.Setup(r => r.AsQueryable()).Returns(mockSet);

        // Also mock GetByCodeAsync just in case
        _productRepositoryMock.Setup(x => x.GetByCodeAsync(productCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        _mapperMock.Setup(m => m.Map<ProductDto>(product)).Returns(dto);

        var handler = new ProductHandlers(
            _productRepositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _mapperMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test Product", result.Value.Name);
    }
}
