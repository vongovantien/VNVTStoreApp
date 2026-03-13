using VNVTStore.Application.Common.Attributes;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Tests;

public class ReferenceAttributeTests
{
    [Fact]
    public void ReferenceAttribute_ShouldBeApplied_ToProductDtoCategoryName()
    {
        // Arrange
        var property = typeof(ProductDto).GetProperty("CategoryName");
        
        // Act
        var attribute = property?.GetCustomAttributes(typeof(ReferenceAttribute), false)
            .FirstOrDefault() as ReferenceAttribute;
        
        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("TblCategory", attribute.TableName);
        Assert.Equal("CategoryCode", attribute.ForeignKey);
        Assert.Equal("Name", attribute.SelectColumn);
    }

    [Fact]
    public void ReferenceCollectionAttribute_ShouldBeApplied_ToProductDtoProductImages()
    {
        // Arrange
        var property = typeof(ProductDto).GetProperty("ProductImages");
        
        // Act
        var attribute = property?.GetCustomAttributes(typeof(ReferenceCollectionAttribute), false)
            .FirstOrDefault() as ReferenceCollectionAttribute;
        
        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("TblFile", attribute.ChildTableName);
        Assert.Equal("MasterCode", attribute.ForeignKey);
        Assert.Equal("Code", attribute.ParentKey);
        Assert.Equal("MasterType", attribute.FilterColumn);
        Assert.Equal("TblProduct", attribute.FilterValue);
        Assert.Equal(typeof(ProductImageDto), attribute.ChildDtoType);
    }

    [Fact]
    public void CategoryDto_ParentName_ShouldHaveReferenceAttribute()
    {
        // Arrange
        var property = typeof(CategoryDto).GetProperty("ParentName");
        
        // Act
        var attribute = property?.GetCustomAttributes(typeof(ReferenceAttribute), false)
            .FirstOrDefault() as ReferenceAttribute;
        
        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("TblCategory", attribute.TableName);
    }

    [Fact]
    public void ReviewDto_UserName_ShouldHaveReferenceAttribute()
    {
        // Arrange
        var property = typeof(ReviewDto).GetProperty("UserName");
        
        // Act
        var attribute = property?.GetCustomAttributes(typeof(ReferenceAttribute), false)
            .FirstOrDefault() as ReferenceAttribute;
        
        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("TblUser", attribute.TableName);
        Assert.Equal("UserCode", attribute.ForeignKey);
        Assert.Equal("FullName", attribute.SelectColumn);
    }
}
