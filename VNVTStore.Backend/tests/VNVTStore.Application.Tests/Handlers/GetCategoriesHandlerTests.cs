using System;
using System.Data;
using System.Data.Common;
using AutoMapper;
using Moq;
using Moq.Protected;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Categories.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace VNVTStore.Application.Tests.Handlers;

public class GetCategoriesHandlerTests
{
    private readonly Mock<IRepository<TblCategory>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDapperContext> _mockDapperContext;
    private readonly Mock<IBaseUrlService> _mockBaseUrlService;
    private readonly Mock<DbConnection> _mockConnection;
    private readonly Mock<DbCommand> _mockCommand;
    private readonly Mock<DbDataReader> _mockDataReader;
    private readonly Mock<DbParameterCollection> _mockParameters;

    private readonly GetCategoriesHandler _handler;

    public GetCategoriesHandlerTests()
    {
        _mockRepository = new Mock<IRepository<TblCategory>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockDapperContext = new Mock<IDapperContext>();
        _mockBaseUrlService = new Mock<IBaseUrlService>();
        
        _mockConnection = new Mock<DbConnection>();
        _mockCommand = new Mock<DbCommand>();
        _mockDataReader = new Mock<DbDataReader>();
        _mockParameters = new Mock<DbParameterCollection>();

        _mockCommand.SetupAllProperties();
        _mockCommand.Protected().Setup<DbParameterCollection>("DbParameterCollection").Returns(_mockParameters.Object);
        var mockParameter = new Mock<DbParameter>();
        _mockCommand.Protected().Setup<DbParameter>("CreateDbParameter").Returns(mockParameter.Object);

        _mockCommand.Protected()
            .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(_mockDataReader.Object);

        _mockConnection.SetupAllProperties();
        _mockConnection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(_mockCommand.Object);
        _mockDapperContext.Setup(c => c.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);
        
        _mockBaseUrlService.Setup(x => x.GetBaseUrl()).Returns("http://test.com");

        _handler = new GetCategoriesHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockDapperContext.Object,
            _mockBaseUrlService.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldMapImageUrlFromFiles()
    {
        // Arrange
        var request = new GetPagedQuery<CategoryDto> { PageIndex = 1, PageSize = 10 };
        var cols = new[] { "TotalRow", "Code", "Name", "ParentCode" };
        var fileCols = new[] { "Code", "MasterCode", "MasterType", "Path" };

        var queryCount = 0;
        _mockCommand.Protected()
            .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => {
                queryCount++;
                return _mockDataReader.Object;
            });

        _mockDataReader.Setup(r => r.FieldCount).Returns(() => queryCount == 1 ? cols.Length : fileCols.Length);
        _mockDataReader.Setup(r => r.GetName(It.IsAny<int>())).Returns((int i) => queryCount == 1 ? cols[i] : fileCols[i]);
        _mockDataReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns((string name) => {
            var c = queryCount == 1 ? cols : fileCols;
            return Array.IndexOf(c, name);
        });
        
        _mockDataReader.Setup(r => r.GetValue(It.IsAny<int>())).Returns((int i) => {
            if (queryCount == 1) {
                return cols[i] switch {
                    "TotalRow" => 1,
                    "Code" => "CAT001",
                    "Name" => "Category 1",
                    "ParentCode" => (object)DBNull.Value,
                    _ => (object)DBNull.Value
                };
            } else {
                return fileCols[i] switch {
                    "Code" => "F001",
                    "MasterCode" => "CAT001",
                    "MasterType" => "Category",
                    "Path" => "cat_image.png",
                    _ => (object)DBNull.Value
                };
            }
        });

        var readCountTotal = 0;
        var readCountPerQuery = 0;
        var lastQueryIndex = 0;

        _mockDataReader.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>())).Returns(() => {
            if (lastQueryIndex != queryCount) {
                readCountPerQuery = 0;
                lastQueryIndex = queryCount;
            }
            readCountPerQuery++;
            return Task.FromResult(readCountPerQuery == 1);
        });

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        var item = result.Value.Items.First();
        Assert.Equal("http://test.com/cat_image.png", item.ImageUrl);
    }
}
