using FluentAssertions;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Constants;
using VNVTStore.Domain.Entities;
using AutoMapper;
using Xunit;

namespace VNVTStore.Application.Tests.Handlers;

/// <summary>
/// Unit tests for Generic Handler operations
/// </summary>
public class GenericHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDapperContext> _dapperMock;

    public GenericHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _dapperMock = new Mock<IDapperContext>();
    }

    [Fact]
    public void GetPagedQuery_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var query = new GetPagedQuery<ProductDto>();
        
        // Assert
        query.PageIndex.Should().Be(AppConstants.Paging.DefaultPageNumber);
        query.PageSize.Should().Be(AppConstants.Paging.DefaultPageSize);
        query.Search.Should().BeNull();
    }

    [Fact]
    public void GetByCodeQuery_ShouldHaveCorrectCode()
    {
        // Arrange
        var code = "TEST-001";
        
        // Act
        var query = new GetByCodeQuery<ProductDto>(code);
        
        // Assert
        query.Code.Should().Be(code);
        query.IncludeChildren.Should().BeFalse();
    }

    [Fact]
    public void CreateCommand_ShouldContainDto()
    {
        // Arrange
        var dto = new CreateProductDto { Name = "Test Product", Price = 100 };
        
        // Act
        var command = new CreateCommand<CreateProductDto, ProductDto>(dto);
        
        // Assert
        command.Dto.Should().NotBeNull();
        command.Dto.Name.Should().Be("Test Product");
        command.Dto.Price.Should().Be(100);
    }

    [Fact]
    public void UpdateCommand_ShouldContainCodeAndDto()
    {
        // Arrange
        var code = "PROD-001";
        var dto = new UpdateProductDto { Name = "Updated Product" };
        
        // Act
        var command = new UpdateCommand<UpdateProductDto, ProductDto>(code, dto);
        
        // Assert
        command.Code.Should().Be(code);
        command.Dto.Should().NotBeNull();
        command.Dto.Name.Should().Be("Updated Product");
    }

    [Fact]
    public void DeleteCommand_ShouldContainCode()
    {
        // Arrange
        var code = "PROD-001";
        
        // Act
        var command = new DeleteCommand<TblProduct>(code);
        
        // Assert
        command.Code.Should().Be(code);
    }

    [Fact]
    public void Result_Success_ShouldHaveValue()
    {
        // Arrange
        var product = new ProductDto { Code = "PROD-001", Name = "Test" };
        
        // Act
        var result = Result<ProductDto>.Success(product);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(product);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Result_Failure_ShouldHaveError()
    {
        // Arrange
        var error = new Error("NotFound", "Product not found");
        
        // Act
        var result = Result<ProductDto>.Failure(error);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void PagedResult_ShouldContainItemsAndTotal()
    {
        // Arrange
        var items = new List<ProductDto>
        {
            new() { Code = "P1", Name = "Product 1" },
            new() { Code = "P2", Name = "Product 2" }
        };
        
        // Act
        var result = new PagedResult<ProductDto>(items, 100);
        
        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(100);
    }

    [Fact]
    public void PagedResult_Empty_ShouldReturnEmptyResult()
    {
        // Act
        var result = PagedResult<ProductDto>.Empty();
        
        // Assert
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }
}
