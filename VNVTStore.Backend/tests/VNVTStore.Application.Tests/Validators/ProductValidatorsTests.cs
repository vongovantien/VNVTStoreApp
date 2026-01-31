using FluentAssertions;
using FluentValidation.TestHelper;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Products.Validators;

namespace VNVTStore.Application.Tests.Validators;

public class ProductValidatorsTests
{
    private readonly CreateProductDtoValidator _createValidator;
    private readonly UpdateProductDtoValidator _updateValidator;

    public ProductValidatorsTests()
    {
        _createValidator = new CreateProductDtoValidator();
        _updateValidator = new UpdateProductDtoValidator();
    }

    [Fact]
    public void CreateProduct_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new CreateProductDto { Name = "", Price = 100 };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateProduct_ShouldFail_WhenPriceIsZeroOrNegative()
    {
        // Arrange
        var dto = new CreateProductDto { Name = "Test Product", Price = 0 };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void CreateProduct_ShouldPass_WhenValid()
    {
        // Arrange
        var dto = new CreateProductDto 
        { 
            Name = "Test Product", 
            Price = 100,
            StockQuantity = 10
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateProduct_ShouldFail_WhenNameExceedsMaxLength()
    {
        // Arrange
        var dto = new CreateProductDto 
        { 
            Name = new string('a', 201), 
            Price = 100 
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateProduct_ShouldFail_WhenStockQuantityIsNegative()
    {
        // Arrange
        var dto = new CreateProductDto 
        { 
            Name = "Test", 
            Price = 100,
            StockQuantity = -1
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity);
    }

    [Fact]
    public void UpdateProduct_ShouldPass_WhenAllOptional()
    {
        // Arrange
        var dto = new UpdateProductDto();

        // Act
        var result = _updateValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
