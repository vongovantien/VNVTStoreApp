using FluentAssertions;
using VNVTStore.Domain.Entities;
using Xunit;

namespace VNVTStore.Application.Tests.Domain.Entities;

public class TblProductDetailTests
{
    [Fact]
    public void Constructor_Should_Set_DefaultValues()
    {
        // Arrange & Act
        var detail = new TblProductDetail();

        // Assert
        detail.Code.Should().NotBeNullOrEmpty();
        detail.Code.Should().HaveLength(32, "because it uses Guid.ToString('N')");
        detail.IsActive.Should().BeTrue();
        detail.CreatedAt.Should().BeNull(); // Or whatever default is expected
        detail.DetailType.Should().Be(VNVTStore.Domain.Enums.ProductDetailType.SPEC);
    }

    [Fact]
    public void Setting_Properties_Should_Work()
    {
        // Arrange
        var detail = new TblProductDetail();
        var now = DateTime.UtcNow;
        var product = TblProduct.Create("Test Product", 100, 80, 10, "CAT01", 50, "SUP01");

        // Act
        detail.ProductCode = "PROD001";
        detail.SpecName = "Color";
        detail.SpecValue = "Red";
        detail.CreatedAt = now;
        detail.Product = product;

        // Assert
        detail.ProductCode.Should().Be("PROD001");
        detail.SpecName.Should().Be("Color");
        detail.SpecValue.Should().Be("Red");
        detail.CreatedAt.Should().Be(now);
        detail.Product.Should().Be(product);
    }
}
