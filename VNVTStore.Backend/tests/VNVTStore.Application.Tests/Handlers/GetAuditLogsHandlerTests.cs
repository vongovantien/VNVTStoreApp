using System;
using System.Data;
using System.Data.Common;
using AutoMapper;
using Moq;
using Moq.Protected;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.AuditLogs.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace VNVTStore.Application.Tests.Handlers;

public class GetAuditLogsHandlerTests
{
    private readonly Mock<IRepository<TblAuditLog>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDapperContext> _mockDapperContext;
    private readonly Mock<DbConnection> _mockConnection;
    private readonly Mock<DbCommand> _mockCommand;
    private readonly Mock<DbDataReader> _mockDataReader;
    private readonly Mock<DbParameterCollection> _mockParameters;

    private readonly GetAuditLogsHandler _handler;

    public GetAuditLogsHandlerTests()
    {
        _mockRepository = new Mock<IRepository<TblAuditLog>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockDapperContext = new Mock<IDapperContext>();
        
        _mockConnection = new Mock<DbConnection>();
        _mockCommand = new Mock<DbCommand>();
        _mockDataReader = new Mock<DbDataReader>();
        _mockParameters = new Mock<DbParameterCollection>();

        _mockCommand.Protected().Setup<DbParameterCollection>("DbParameterCollection").Returns(_mockParameters.Object);
        var mockParameter = new Mock<DbParameter>();
        _mockCommand.Protected().Setup<DbParameter>("CreateDbParameter").Returns(mockParameter.Object);

        _mockCommand.Protected()
            .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(_mockDataReader.Object);

        _mockConnection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(_mockCommand.Object);
        _mockDapperContext.Setup(c => c.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

        _handler = new GetAuditLogsHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockDapperContext.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedAuditLogs()
    {
        // Arrange
        var request = new GetPagedQuery<AuditLogDto> 
        { 
            PageIndex = 1, 
            PageSize = 10,
            Searching = new List<SearchDTO> 
            { 
                new SearchDTO { SearchField = "Action", SearchValue = "Login", SearchCondition = SearchCondition.Contains }
            }
        };

        var cols = new[] { "TotalRow", "Code", "Action", "Detail", "CreatedAt", "UserCode", "UserName", "IpAddress" };
        _mockDataReader.Setup(r => r.FieldCount).Returns(cols.Length);
        _mockDataReader.Setup(r => r.GetName(It.IsAny<int>())).Returns((int i) => cols[i]);
        _mockDataReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns((string name) => Array.IndexOf(cols, name));
        
        _mockDataReader.Setup(r => r.GetValue(It.IsAny<int>())).Returns((int i) => 
            cols[i] switch {
                "TotalRow" => 1,
                "Code" => "A001",
                "Action" => "Login",
                "Detail" => "User logged in",
                "CreatedAt" => DateTime.Now,
                "UserCode" => "U001",
                "UserName" => "testuser",
                "IpAddress" => "127.0.0.1",
                _ => (object)DBNull.Value
            });

        var readCount = 0;
        _mockDataReader.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>())).Returns(() => {
            readCount++;
            return Task.FromResult(readCount == 1);
        });

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value!.Items);
        Assert.Equal("A001", result.Value.Items.First().Code);
        Assert.Equal("Login", result.Value.Items.First().Action);
        Assert.Equal(1, result.Value.TotalItems);
    }
}
