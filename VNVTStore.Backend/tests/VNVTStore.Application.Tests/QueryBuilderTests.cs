using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Tests;

public class QueryBuilderTests
{
    [Fact]
    public void BuildRawQueryPaging_WithNoSearchFields_ShouldGenerateBasicQuery()
    {
        // Arrange
        var tableName = "TblProduct";
        var pageSize = 10;
        var pageIndex = 1;
        var sortDTO = new SortDTO { SortBy = "CreatedAt", Sort = "DESC" };
        var searchFields = new List<SearchDTO>();

        // Act
        var sql = QueryBuilder.BuildRawQueryPaging(pageSize, pageIndex, tableName, null, searchFields, sortDTO, null);

        // Assert
        Assert.NotNull(sql);
        Assert.Contains("TblProduct", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("LIMIT", sql);
        Assert.Contains("OFFSET", sql);
    }

    [Fact]
    public void BuildRawQueryPaging_WithSearchField_ShouldIncludeWhereClause()
    {
        // Arrange
        var tableName = "TblProduct";
        var pageSize = 10;
        var pageIndex = 1;
        var sortDTO = new SortDTO { SortBy = "CreatedAt", Sort = "DESC" };
        var searchFields = new List<SearchDTO>
        {
            new SearchDTO 
            { 
                SearchField = "Name", 
                SearchCondition = SearchCondition.Contains, 
                SearchValue = "test" 
            }
        };

        // Act
        var sql = QueryBuilder.BuildRawQueryPaging(pageSize, pageIndex, tableName, null, searchFields, sortDTO, null);

        // Assert
        Assert.NotNull(sql);
        Assert.Contains("Name", sql);
        Assert.Contains("ILIKE", sql);
    }

    [Fact]
    public void BuildRawQueryPaging_WithReferenceTables_ShouldIncludeJoins()
    {
        // Arrange
        var tableName = "TblProduct";
        var pageSize = 10;
        var pageIndex = 1;
        var sortDTO = new SortDTO { SortBy = "CreatedAt", Sort = "DESC" };
        var searchFields = new List<SearchDTO>();
        var referenceTables = new List<ReferenceTable>
        {
            new ReferenceTable
            {
                TableName = "TblCategory",
                ForeignKeyCol = "CategoryCode",
                ColumnName = "Name",
                AliasName = "CategoryName"
            }
        };

        // Act
        var sql = QueryBuilder.BuildRawQueryPaging(pageSize, pageIndex, tableName, referenceTables, searchFields, sortDTO, null);

        // Assert
        Assert.NotNull(sql);
        Assert.Contains("TblCategory", sql);
        Assert.Contains("LEFT JOIN", sql);
    }

    [Fact]
    public void BuildRawQueryPaging_WithDescendingSort_ShouldHaveDescOrder()
    {
        // Arrange
        var tableName = "TblProduct";
        var pageSize = 10;
        var pageIndex = 1;
        var sortDTO = new SortDTO { SortBy = "Price", Sort = "DESC" };
        var searchFields = new List<SearchDTO>();

        // Act
        var sql = QueryBuilder.BuildRawQueryPaging(pageSize, pageIndex, tableName, null, searchFields, sortDTO, null);

        // Assert
        Assert.NotNull(sql);
        Assert.Contains("DESC", sql);
    }

    [Fact]
    public void BuildRawQueryPaging_WithPagination_ShouldCalculateCorrectOffset()
    {
        // Arrange
        var tableName = "TblProduct";
        var pageSize = 10;
        var pageIndex = 3; // Page 3
        var sortDTO = new SortDTO { SortBy = "CreatedAt", Sort = "DESC" };
        var searchFields = new List<SearchDTO>();

        // Act
        var sql = QueryBuilder.BuildRawQueryPaging(pageSize, pageIndex, tableName, null, searchFields, sortDTO, null);

        // Assert
        Assert.NotNull(sql);
        // SQL format uses: OFFSET (pageIndex - 1) * pageSize, so for page 3: OFFSET (3 - 1) * 10
        Assert.Contains("OFFSET (3 - 1) * 10", sql);
        Assert.Contains("LIMIT 10", sql);
    }
}
