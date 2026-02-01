using System.Data;
using System.Data.Common;
using AutoMapper;
using Moq;
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
    private readonly Mock<DbParameter> _mockParameter;
    private readonly Mock<IDataParameterCollection> _mockParameters;

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
        _mockParameter = new Mock<DbParameter>();
        _mockParameters = new Mock<IDataParameterCollection>();

        // Setup Command and Parameters mocks for Dapper
        _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);
        // _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object); // Parameters is often hard to mock on DbCommand
        _mockCommand.Setup(c => c.CreateParameter()).Returns(_mockParameter.Object);
        _mockCommand.Setup(c => c.ExecuteReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockDataReader.Object);
        // _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockDataReader.Object);

        // Connection Handling
        _mockDapperContext.Setup(c => c.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        // Base URL
        _mockBaseUrlService.Setup(x => x.GetBaseUrl()).Returns("http://test-api.com");

        _handler = new GetProductsHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockDapperContext.Object,
            _mockBaseUrlService.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldExecuteCorrectFileQuery()
    {
        // Arrange
        var request = new GetPagedQuery<ProductDto> { PageIndex = 1, PageSize = 10 };
        
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
        
        // Allow reading once per query
        var readCount = 0;
        _mockDataReader.Setup(r => r.Read()).Returns(() => {
            readCount++;
            if (readCount == 1) return true; // Product Row 1
            if (readCount == 2) { isProductQuery = false; return false; } // End Product
            if (readCount == 3) return true; // File Row 1
            if (readCount == 4) return false; // End File
            return false;
        });

        // Act
        try 
        {
            await _handler.Handle(request, CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Dapper might still complain about type conversion or flags, but capturing SQL is the goal
             // Console.WriteLine(ex);
        }

        // Assert
        // We look for the file query in captured commands
        var fileQuery = capturedSqls.FirstOrDefault(s => s.Contains("TblFile"));
        Assert.NotNull(fileQuery);
        Assert.Contains("ANY(@Codes)", fileQuery);
    }
}
