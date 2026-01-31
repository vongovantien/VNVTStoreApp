using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Tests;

public class QueryHelperTests
{
    #region Test Data Classes
    
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Value { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region Test Fixtures

    private readonly IQueryable<TestEntity> _data;

    public QueryHelperTests()
    {
        _data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alpha Product", Status = "Active", Value = 10, Price = 99.99m, CreatedAt = new DateTime(2024, 1, 15), Category = "Electronics", IsActive = true },
            new() { Id = 2, Name = "Beta Service", Status = "Inactive", Value = 20, Price = 49.99m, CreatedAt = new DateTime(2024, 2, 20), Category = "Services", IsActive = false },
            new() { Id = 3, Name = "Gamma Tool", Status = "Active", Value = 30, Price = 149.99m, CreatedAt = new DateTime(2024, 3, 10), Category = "Electronics", IsActive = true },
            new() { Id = 4, Name = "Delta Item", Status = "Pending", Value = 40, Price = 199.99m, CreatedAt = new DateTime(2024, 4, 5), Category = null, IsActive = false },
            new() { Id = 5, Name = "Epsilon Widget", Status = "Active", Value = 50, Price = 29.99m, CreatedAt = new DateTime(2024, 5, 25), Category = "Accessories", IsActive = true },
            new() { Id = 6, Name = "Zeta Gadget", Status = "Inactive", Value = 60, Price = 79.99m, CreatedAt = new DateTime(2024, 6, 30), Category = "Electronics", IsActive = false },
        }.AsQueryable();
    }

    #endregion

    #region Basic Filtering Tests

    [Fact]
    public void ApplyFilters_EmptyFilters_ReturnsAllData()
    {
        // Arrange
        var filters = new List<SearchDTO>();

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(6, result.Count);
    }

    [Fact]
    public void ApplyFilters_NullFilters_ReturnsAllData()
    {
        // Act
        var result = QueryHelper.ApplyFilters(_data, null).ToList();

        // Assert
        Assert.Equal(6, result.Count);
    }

    [Fact]
    public void ApplyFilters_SingleEqualCondition_FiltersCorrectly()
    {
        // Arrange
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Status", SearchCondition = SearchCondition.Equal, SearchValue = "Active" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, x => Assert.Equal("Active", x.Status));
    }

    [Fact]
    public void ApplyFilters_ContainsCondition_FiltersCorrectly()
    {
        // Arrange - Search for names containing "Product" or "Service"
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Name", SearchCondition = SearchCondition.Contains, SearchValue = "Product" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Alpha Product", result[0].Name);
    }

    [Fact]
    public void ApplyFilters_NotEqualCondition_FiltersCorrectly()
    {
        // Arrange
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Status", SearchCondition = SearchCondition.NotEqual, SearchValue = "Active" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, x => Assert.NotEqual("Active", x.Status));
    }

    #endregion

    #region Numeric Comparison Tests

    [Fact]
    public void ApplyFilters_GreaterThan_FiltersCorrectly()
    {
        // Arrange
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Value", SearchCondition = SearchCondition.GreaterThan, SearchValue = 35 }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(3, result.Count); // Delta (40), Epsilon (50), Zeta (60)
        Assert.All(result, x => Assert.True(x.Value > 35));
    }

    [Fact]
    public void ApplyFilters_LessThanEqual_FiltersCorrectly()
    {
        // Arrange
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Price", SearchCondition = SearchCondition.LessThanEqual, SearchValue = 79.99m }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(3, result.Count); // Beta (49.99), Epsilon (29.99), Zeta (79.99)
    }

    [Fact]
    public void ApplyFilters_CombinedRangeConditions_FiltersCorrectly()
    {
        // Arrange - Value between 20 and 50 (inclusive)
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Value", SearchCondition = SearchCondition.GreaterThanEqual, SearchValue = 20 },
            new() { SearchField = "Value", SearchCondition = SearchCondition.LessThanEqual, SearchValue = 50 }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(4, result.Count); // Beta (20), Gamma (30), Delta (40), Epsilon (50)
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public void ApplyFilters_IsNullCondition_FiltersCorrectly()
    {
        // Arrange - Find items with null Category
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Category", SearchCondition = SearchCondition.IsNull, SearchValue = null }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Delta Item", result[0].Name);
    }

    [Fact]
    public void ApplyFilters_IsNotNullCondition_FiltersCorrectly()
    {
        // Arrange
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Category", SearchCondition = SearchCondition.IsNotNull, SearchValue = null }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.All(result, x => Assert.NotNull(x.Category));
    }

    #endregion

    #region Group Filtering Tests (OR Logic)

    [Fact]
    public void ApplyFilters_SingleGroupWithOrCondition_FiltersCorrectly()
    {
        // Arrange - (Status = Active OR Status = Pending)
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Status", SearchCondition = SearchCondition.Equal, SearchValue = "Active", GroupID = 1, CombineCondition = "OR" },
            new() { SearchField = "Status", SearchCondition = SearchCondition.Equal, SearchValue = "Pending", GroupID = 1, CombineCondition = "OR" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(4, result.Count); // Alpha, Gamma, Delta, Epsilon
    }

    [Fact]
    public void ApplyFilters_GroupWithMixedConditionTypes_FiltersCorrectly()
    {
        // Arrange - (Category = Electronics OR Price < 50)
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Category", SearchCondition = SearchCondition.Equal, SearchValue = "Electronics", GroupID = 1, CombineCondition = "OR" },
            new() { SearchField = "Price", SearchCondition = SearchCondition.LessThan, SearchValue = 50m, GroupID = 1, CombineCondition = "OR" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        // Electronics: Alpha (99.99), Gamma (149.99), Zeta (79.99)
        // Price < 50: Beta (49.99), Epsilon (29.99)
        // Union: Alpha, Beta, Gamma, Epsilon, Zeta = 5
        Assert.Equal(5, result.Count);
    }

    #endregion

    #region Complex Combined Filtering Tests

    [Fact]
    public void ApplyFilters_GlobalAndWithGroupedOr_FiltersCorrectly()
    {
        // Arrange
        // Global: IsActive = true (Alpha, Gamma, Epsilon)
        // Group: (Value > 25 OR Category = Accessories)
        // Expected: Gamma (30, Electronics, Active), Epsilon (50, Accessories, Active)
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "IsActive", SearchCondition = SearchCondition.Equal, SearchValue = true, GroupID = null },
            new() { SearchField = "Value", SearchCondition = SearchCondition.GreaterThan, SearchValue = 25, GroupID = 1, CombineCondition = "OR" },
            new() { SearchField = "Category", SearchCondition = SearchCondition.Equal, SearchValue = "Accessories", GroupID = 1, CombineCondition = "OR" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Name == "Gamma Tool");
        Assert.Contains(result, x => x.Name == "Epsilon Widget");
    }

    [Fact]
    public void ApplyFilters_MultipleGlobalAndConditions_FiltersCorrectly()
    {
        // Arrange - Status = Active AND Category = Electronics
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Status", SearchCondition = SearchCondition.Equal, SearchValue = "Active" },
            new() { SearchField = "Category", SearchCondition = SearchCondition.Equal, SearchValue = "Electronics" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(2, result.Count); // Alpha, Gamma
        Assert.All(result, x => Assert.Equal("Active", x.Status));
        Assert.All(result, x => Assert.Equal("Electronics", x.Category));
    }

    [Fact]
    public void ApplyFilters_MultipleGroups_AndBetweenGroups()
    {
        // Arrange
        // Group 1 (OR): Status = Active OR Status = Inactive -> Alpha, Beta, Gamma, Epsilon, Zeta
        // Group 2 (OR): Value < 25 OR Value > 45 -> Alpha (10), Beta (20), Epsilon (50), Zeta (60)
        // Intersection: Alpha, Beta, Epsilon, Zeta
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Status", SearchCondition = SearchCondition.Equal, SearchValue = "Active", GroupID = 1, CombineCondition = "OR" },
            new() { SearchField = "Status", SearchCondition = SearchCondition.Equal, SearchValue = "Inactive", GroupID = 1, CombineCondition = "OR" },
            
            new() { SearchField = "Value", SearchCondition = SearchCondition.LessThan, SearchValue = 25, GroupID = 2, CombineCondition = "OR" },
            new() { SearchField = "Value", SearchCondition = SearchCondition.GreaterThan, SearchValue = 45, GroupID = 2, CombineCondition = "OR" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Contains(result, x => x.Name == "Alpha Product");
        Assert.Contains(result, x => x.Name == "Beta Service");
        Assert.Contains(result, x => x.Name == "Epsilon Widget");
        Assert.Contains(result, x => x.Name == "Zeta Gadget");
    }

    [Fact]
    public void ApplyFilters_ThreeGroupsWithGlobalCondition_ComplexScenario()
    {
        // Arrange
        // Global: Price > 30
        // Group 1: (Status = Active OR Status = Pending)
        // Group 2: (Category = Electronics OR Category = Services)
        // 
        // Price > 30: Alpha (99.99), Beta (49.99), Gamma (149.99), Delta (199.99), Zeta (79.99)
        // Status: Alpha, Gamma, Delta, Epsilon (from Active), Delta (from Pending)
        // Category: Alpha, Gamma, Zeta (Electronics), Beta (Services)
        // 
        // Intersection: Alpha (99.99, Active, Electronics), Gamma (149.99, Active, Electronics)
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Price", SearchCondition = SearchCondition.GreaterThan, SearchValue = 30m, GroupID = null },
            
            new() { SearchField = "Status", SearchCondition = SearchCondition.Equal, SearchValue = "Active", GroupID = 1, CombineCondition = "OR" },
            new() { SearchField = "Status", SearchCondition = SearchCondition.Equal, SearchValue = "Pending", GroupID = 1, CombineCondition = "OR" },
            
            new() { SearchField = "Category", SearchCondition = SearchCondition.Equal, SearchValue = "Electronics", GroupID = 2, CombineCondition = "OR" },
            new() { SearchField = "Category", SearchCondition = SearchCondition.Equal, SearchValue = "Services", GroupID = 2, CombineCondition = "OR" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Name == "Alpha Product");
        Assert.Contains(result, x => x.Name == "Gamma Tool");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void ApplyFilters_InvalidFieldName_IgnoresFilter()
    {
        // Arrange
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "NonExistentField", SearchCondition = SearchCondition.Equal, SearchValue = "test" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert - Should return all data since filter is ignored
        Assert.Equal(6, result.Count);
    }

    [Fact]
    public void ApplyFilters_EmptySearchField_IgnoresFilter()
    {
        // Arrange
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "", SearchCondition = SearchCondition.Equal, SearchValue = "test" },
            new() { SearchField = "Status", SearchCondition = SearchCondition.Equal, SearchValue = "Active" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert - Only the valid filter should apply
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplyFilters_NullSearchValue_IgnoresFilter()
    {
        // Arrange
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Name", SearchCondition = SearchCondition.Contains, SearchValue = null }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert - Filter ignored, returns all
        Assert.Equal(6, result.Count);
    }

    [Fact]
    public void ApplyFilters_CaseInsensitiveFieldName_FiltersCorrectly()
    {
        // Arrange - Use lowercase field name
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "status", SearchCondition = SearchCondition.Equal, SearchValue = "Active" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplyFilters_GroupWithAndCondition_FiltersCorrectly()
    {
        // Arrange - Group with AND (default)
        // (Category = Electronics AND IsActive = true)
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "Category", SearchCondition = SearchCondition.Equal, SearchValue = "Electronics", GroupID = 1, CombineCondition = "AND" },
            new() { SearchField = "IsActive", SearchCondition = SearchCondition.Equal, SearchValue = true, GroupID = 1, CombineCondition = "AND" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert - Alpha, Gamma (both Electronics AND Active)
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ApplyFilters_EmptyGroupReturnsAllData()
    {
        // Arrange - Group with no valid filters
        var filters = new List<SearchDTO>
        {
            new() { SearchField = "", SearchCondition = SearchCondition.Equal, SearchValue = "test", GroupID = 1, CombineCondition = "OR" }
        };

        // Act
        var result = QueryHelper.ApplyFilters(_data, filters).ToList();

        // Assert
        Assert.Equal(6, result.Count);
    }

    #endregion
}
