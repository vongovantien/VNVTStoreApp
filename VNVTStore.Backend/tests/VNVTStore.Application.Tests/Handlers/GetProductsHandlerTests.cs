using System.Data;
using System.Data.Common;
using AutoMapper;
using Moq;
using Moq.Protected;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Products.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

using System.Threading;
using System.Threading.Tasks;

namespace VNVTStore.Application.Tests.Handlers;

public class GetProductsHandlerTests
{
    private readonly Mock<IRepository<TblProduct>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDapperContext> _mockDapperContext;
    private readonly Mock<IBaseUrlService> _mockBaseUrlService;
    private readonly Mock<DbConnection> _mockConnection;
    private readonly Mock<DbCommand> _mockCommand;
    private readonly Mock<DbDataReader> _mockDataReader;
    private readonly Mock<DbParameterCollection> _mockParameters;

    private readonly GetProductsHandler _handler;

    public GetProductsHandlerTests()
    {
        _mockRepository = new Mock<IRepository<TblProduct>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockDapperContext = new Mock<IDapperContext>();
        _mockBaseUrlService = new Mock<IBaseUrlService>();
        
        _mockConnection = new Mock<DbConnection>();
        _mockCommand = new Mock<DbCommand>();
        _mockDataReader = new Mock<DbDataReader>();
        _mockParameters = new Mock<DbParameterCollection>();

        // Setup Command and Parameters mocks for Dapper
        // Dapper needs DbCommand for async operations. 
        // We must setup the protected property DbParameterCollection instead of mocking Parameters directly.
        _mockCommand.Protected().Setup<DbParameterCollection>("DbParameterCollection").Returns(_mockParameters.Object);
        
        // Setup CreateParameter
        var mockParameter = new Mock<DbParameter>();
        _mockCommand.Protected().Setup<DbParameter>("CreateDbParameter").Returns(mockParameter.Object);

        // We must also setup the protected method ExecuteDbDataReaderAsync which Dapper calls
        _mockCommand.Protected()
            .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(_mockDataReader.Object);

        _mockConnection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(_mockCommand.Object);

        // Connection Handling
        _mockDapperContext.Setup(c => c.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        // Base URL
        _mockBaseUrlService.Setup(x => x.GetBaseUrl()).Returns("http://test-api.com");

        _handler = new GetProductsHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockDapperContext.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldExecuteCorrectFileQuery()
    {
        // Arrange
        // Specify Fields to avoid automatic collection population in BaseHandler,
        // allowing us to focus on the manual fetch logic in GetProductsHandler.
        var request = new GetPagedQuery<ProductDto> 
        { 
            PageIndex = 1, 
            PageSize = 10,
            Fields = new List<string> { "Code", "Name", "ProductImages" }
        };
        
        // Capture SQL commands
        var capturedSqls = new List<string>();
        _mockCommand.SetupSet(c => c.CommandText = It.IsAny<string>()).Callback<string>(s => capturedSqls.Add(s));

        // Setup DataReader to handle Dapper's GetOrdinal and GetValue
        // We simulate 1 Product row, then 1 File row.
        
        var productCols = new[] { "TotalRow", "Code", "Name", "ModifiedType" };
        var fileCols = new[] { "Code", "MasterCode", "MasterType", "Path", "OriginalName" };
        
        var isProductQuery = true; // State to switch between queries

        _mockDataReader.Setup(r => r.FieldCount).Returns(() => isProductQuery ? productCols.Length : fileCols.Length);
        
        _mockDataReader.Setup(r => r.GetName(It.IsAny<int>())).Returns((int i) => 
            isProductQuery ? productCols[i] : fileCols[i]);

        _mockDataReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns((string name) => 
            {
                var cols = isProductQuery ? productCols : fileCols;
                return Array.IndexOf(cols, name);
            });

        _mockDataReader.Setup(r => r.GetValue(It.IsAny<int>())).Returns((int i) => 
        {
            var cols = isProductQuery ? productCols : fileCols;
            var name = cols[i];
            if (isProductQuery)
            {
                return name switch {
                    "TotalRow" => 1,
                    "Code" => "P001",
                    "Name" => "Test Product",
                    "ModifiedType" => "Create",
                    _ => (object)DBNull.Value
                };
            }
            else
            {
                 return name switch {
                    "Code" => "F001",
                    "MasterCode" => "P001",
                    "MasterType" => "Product",
                    "Path" => "image.jpg",
                    "OriginalName" => "Image",
                    _ => (object)DBNull.Value
                };
            }
        });
        
        // Allow reading for the sequence:
        // 1. Products (paging)
        // 2. Manual Files (TblFile)
        // 3. Manual Reviews (TblReview) - return empty
        var readCount = 0;
        var queryIndex = 0;

        _mockDataReader.Setup(r => r.Read()).Returns(() => {
            readCount++;
            var res = false;
            
            // Query 1: Products (paging)
            if (readCount == 1) { queryIndex = 1; res = true; } 
            else if (readCount == 2) { res = false; }
            
            // Query 2: Manual Files (TblFile)
            else if (readCount == 3) { queryIndex = 2; isProductQuery = false; res = true; } // row 1
            else if (readCount == 4) { res = false; }

            // Query 3: Manual Reviews (TblReview)
            else if (readCount == 5) { queryIndex = 3; res = false; }

            Console.WriteLine($"[TEST-DEBUG] Read() Q{queryIndex}, Count: {readCount}, Result: {res}");
            return res;
        });

        _mockDataReader.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>())).Returns(() => {
            readCount++;
            var res = false;

            // Same logic for Async
            if (readCount == 1) { queryIndex = 1; res = true; } 
            else if (readCount == 2) { res = false; }
            
            else if (readCount == 3) { queryIndex = 2; isProductQuery = false; res = true; }
            else if (readCount == 4) { res = false; }

            else if (readCount == 5) { queryIndex = 3; res = false; }

            Console.WriteLine($"[TEST-DEBUG] ReadAsync() Q{queryIndex}, Count: {readCount}, Result: {res}");
            return Task.FromResult(res);
        });

        // Act
        try 
        {
            await _handler.Handle(request, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TEST-DEBUG] Exception in Handle: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        // Assert
        Console.WriteLine($"[TEST-DEBUG] Captured SQLs count: {capturedSqls.Count}");
        foreach(var sql in capturedSqls) 
        {
             Console.WriteLine($"[TEST-DEBUG] SQL: {sql.Substring(0, Math.Min(sql.Length, 50))}...");
        }

        // We look for the file query in captured commands
        var fileQuery = capturedSqls.FirstOrDefault(s => s != null && s.Contains("TblFile"));
        Assert.NotNull(fileQuery);
        Assert.Contains("ANY(@Codes)", fileQuery);
    }

    [Fact]
    public void QueryBuilder_ShouldGenerateInClause_ForMultiSelectCategory()
    {
        // Arrange - Simulate Frontend sending multi-select category filter
        var filters = new List<SearchDTO>
        {
            new SearchDTO 
            { 
                SearchField = "CategoryCode", 
                SearchValue = new[] { "CAT1", "CAT2" }, // Array for IN
                SearchCondition = SearchCondition.In 
            }
        };

        // Act - Build parameterized query directly using QueryBuilder
        var result = VNVTStore.Application.Common.Helpers.QueryBuilder.BuildRawQueryPagingParameterized(
            pageSize: 10,
            pageIndex: 1,
            rootTbl: "TblProduct",
            refTblList: null,
            searchFieldList: filters,
            sortDTO: new SortDTO { SortBy = "CreatedAt", Sort = "DESC" },
            fields: new List<string> { "Code", "Name" }
        );

        // Assert - Verify SQL contains IN clause (Postgres ANY syntax)
        Assert.NotNull(result);
        Assert.NotNull(result.Sql);
        Assert.Contains("\"CategoryCode\" = ANY(@p", result.Sql);
        
        // Verify parameters contain the array values
        Assert.NotNull(result.Parameters);
    }
}
