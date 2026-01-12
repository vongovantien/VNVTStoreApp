using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VNVTStore.Application.Common;

namespace VNVTStore.Tests.Common;

public class QueryHelperTests
{
    private class TestEntity
    {
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public TestNested? Nested { get; set; }
        public List<TestItem> Items { get; set; } = new();
    }

    private class TestNested
    {
        public string Description { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
    
    private class TestItem
    {
        public string ItemName { get; set; } = null!;
        public decimal Price { get; set; }
    }

    [Fact]
    public void ApplySelection_ShouldSelectSimpleFields()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Name = "Test1", Age = 10, Nested = new TestNested { Description = "Desc1" } },
            new() { Name = "Test2", Age = 20, Nested = new TestNested { Description = "Desc2" } }
        }.AsQueryable();

        var fields = new List<string> { "Name" };

        // Act
        var resultQuery = QueryHelper.ApplySelection(data, fields);
        var result = resultQuery.ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Test1", result[0].Name);
        Assert.Equal(0, result[0].Age); // Should be default
        Assert.Null(result[0].Nested);
    }

    [Fact]
    public void ApplySelection_ShouldSelectNestedFields()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { 
                Name = "Test1", 
                Nested = new TestNested { Description = "Desc1", Code = "C1" } 
            }
        }.AsQueryable();

        var fields = new List<string> { "Nested.Description" };

        // Act
        var resultQuery = QueryHelper.ApplySelection(data, fields);
        var result = resultQuery.ToList();

        // Assert
        Assert.NotNull(result[0].Nested);
        Assert.Equal("Desc1", result[0].Nested!.Description);
        Assert.Null(result[0].Nested.Code); // Should be null
        Assert.Null(result[0].Name);
    }

    [Fact]
    public void ApplySelection_ShouldSelectCollectionFields()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { 
                Name = "Test1", 
                Items = new List<TestItem> 
                { 
                    new() { ItemName = "Item1", Price = 100 },
                    new() { ItemName = "Item2", Price = 200 }
                }
            }
        }.AsQueryable();

        var fields = new List<string> { "Items.ItemName" };

        // Act
        var resultQuery = QueryHelper.ApplySelection(data, fields);
        var result = resultQuery.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Items.Count);
        Assert.Equal("Item1", result[0].Items[0].ItemName);
        Assert.Equal(0, result[0].Items[0].Price); // Default
    }

    [Fact]
    public void ApplySelection_ShouldSelectProductCodeAndName()
    {
        // User requested scenario: "lấy list product những chỉ lấy , code, Name thôi"
        // Arrange
        var products = new List<TestProduct>
        {
            new() { Code = "P001", Name = "Product 1", Price = 100, Description = "Desc 1" },
            new() { Code = "P002", Name = "Product 2", Price = 200, Description = "Desc 2" }
        }.AsQueryable();

        var fields = new List<string> { "Code", "Name" };

        // Act
        var resultQuery = QueryHelper.ApplySelection(products, fields);
        var result = resultQuery.ToList();

        // Assert
        Assert.Equal(2, result.Count);
        
        Assert.Equal("P001", result[0].Code);
        Assert.Equal("Product 1", result[0].Name);
        Assert.Null(result[0].Description); // Should be null (default)
        Assert.Equal(0, result[0].Price);   // Should be 0 (default)
        
        Assert.Equal("P002", result[1].Code);
        Assert.Equal("Product 2", result[1].Name);
    }
    
    private class TestProduct
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }
}
