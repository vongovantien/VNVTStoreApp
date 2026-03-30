using System.Data;
using AutoMapper;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;
using VNVTStore.Application.Common.Helpers;
using System.Text;
using VNVTStore.Domain.Enums;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace VNVTStore.Application.Tests;

public class BaseHandlerLogicTests
{
    private readonly Mock<IRepository<TblProduct>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDapperContext> _mockDapperContext;

    public BaseHandlerLogicTests()
    {
        _mockRepository = new Mock<IRepository<TblProduct>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockDapperContext = new Mock<IDapperContext>();
    }

    public class TestableBaseHandler : BaseHandler<TblProduct>
    {
        public TestableBaseHandler(
            IRepository<TblProduct> repository, 
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IDapperContext dapperContext) 
            : base(repository, unitOfWork, mapper, dapperContext)
        {
        }

        public async Task<Result<int>> TestImportAsync<TImportDto, TResponse>(
            Stream fileStream, 
            CancellationToken cancellationToken,
            Func<TImportDto, TblProduct?, Task>? onBeforeSave = null) 
            where TImportDto : class, new()
            where TResponse : class, new()
        {
            return await ImportAsync<TImportDto, TResponse>(fileStream, cancellationToken, onBeforeSave);
        }
    }

    [Fact]
    public async Task ImportAsync_WithEmptyStream_ShouldReturnFailure()
    {
        // Arrange
        var handler = new TestableBaseHandler(_mockRepository.Object, _mockUnitOfWork.Object, _mockMapper.Object, _mockDapperContext.Object);
        var stream = new MemoryStream();

        // Act
        var result = await handler.TestImportAsync<CreateProductDto, ProductDto>(stream, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        // The error might be license related IF not set, but the helper sets it.
        // If the stream is truly empty, EPPlus might throw a different error.
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void GetEntityDisplayName_ShouldReturnCorrectProperty()
    {
        // Arrange
        var handler = new TestableBaseHandler(_mockRepository.Object, _mockUnitOfWork.Object, _mockMapper.Object, _mockDapperContext.Object);
        var product = TblProduct.Create("Test Product", 100, 80, 10, "CAT01", 50, "SUP01");

        // Act
        var method = typeof(BaseHandler<TblProduct>).GetMethod("GetEntityDisplayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var displayName = method!.Invoke(handler, new object[] { product }) as string;

        // Assert
        Assert.Equal("Test Product", displayName);
    }

    [Fact]
    public void GetEntityDisplayName_ShouldFallbackToCode()
    {
        // Arrange
        var handler = new TestableBaseHandler(_mockRepository.Object, _mockUnitOfWork.Object, _mockMapper.Object, _mockDapperContext.Object);
        var product = TblProduct.Create("", 100, 80, 10, "CAT01", 50, "SUP01");

        // Act
        var method = typeof(BaseHandler<TblProduct>).GetMethod("GetEntityDisplayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var displayName = method!.Invoke(handler, new object[] { product }) as string;

        // Assert
        Assert.Equal(product.Code, displayName);
    }

    [Fact]
    public void FilterAndValidateFields_ShouldAlwaysIncludeCode()
    {
        // Arrange
        var handler = new TestableBaseHandler(_mockRepository.Object, _mockUnitOfWork.Object, _mockMapper.Object, _mockDapperContext.Object);
        var fields = new List<string> { "Name", "Price" };

        // Act
        var method = typeof(BaseHandler<TblProduct>).GetMethod("FilterAndValidateFields", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            !.MakeGenericMethod(typeof(ProductDto));
        var validFields = method.Invoke(handler, new object[] { fields, new List<ReferenceTable>() }) as List<string>;

        // Assert
        Assert.NotNull(validFields);
        Assert.Contains("Code", validFields);
    }
}
