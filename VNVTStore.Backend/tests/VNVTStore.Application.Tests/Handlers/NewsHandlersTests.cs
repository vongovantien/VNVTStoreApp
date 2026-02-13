using System;
using System.Data;
using System.Data.Common;
using AutoMapper;
using Moq;
using Moq.Protected;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.News.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace VNVTStore.Application.Tests.Handlers;

public class NewsHandlersTests
{
    private readonly Mock<IRepository<TblNews>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDapperContext> _mockDapperContext;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<DbConnection> _mockConnection;
    private readonly Mock<DbCommand> _mockCommand;
    private readonly Mock<DbDataReader> _mockDataReader;
    private readonly Mock<DbParameterCollection> _mockParameters;

    private readonly NewsHandlers _handler;

    public NewsHandlersTests()
    {
        _mockRepository = new Mock<IRepository<TblNews>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockDapperContext = new Mock<IDapperContext>();
        _mockFileService = new Mock<IFileService>();
        
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

        _handler = new NewsHandlers(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockDapperContext.Object,
            _mockFileService.Object
        );
    }

    [Fact]
    public async Task Create_ShouldGenerateSlugAndSaveImages()
    {
        // Arrange
        var dto = new CreateNewsDto 
        { 
            Title = "Test News", 
            Content = "Test Content", 
            Thumbnail = "image_data" 
        };
        var command = new CreateCommand<CreateNewsDto, NewsDto>(dto);

        var newsEntity = new TblNews { Title = dto.Title };
        _mockMapper.Setup(m => m.Map<TblNews>(dto)).Returns(newsEntity);
        _mockFileService.Setup(f => f.SaveAndLinkImagesAsync(It.IsAny<string>(), "News", It.IsAny<string[]>(), "news", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((IEnumerable<string>)new[] { "saved_image.jpg" }));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("test-news", newsEntity.Slug);
        Assert.Equal("saved_image.jpg", newsEntity.Thumbnail);
        _mockRepository.Verify(r => r.AddAsync(newsEntity, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GetPaged_ShouldReturnNews()
    {
        // Arrange
        var request = new GetPagedQuery<NewsDto> { PageIndex = 1, PageSize = 10 };

        var cols = new[] { "TotalRow", "Code", "Title", "Slug", "CreatedAt" };
        _mockDataReader.Setup(r => r.FieldCount).Returns(cols.Length);
        _mockDataReader.Setup(r => r.GetName(It.IsAny<int>())).Returns((int i) => cols[i]);
        _mockDataReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns((string name) => Array.IndexOf(cols, name));
        
        _mockDataReader.Setup(r => r.GetValue(It.IsAny<int>())).Returns((int i) => 
            cols[i] switch {
                "TotalRow" => 1,
                "Code" => "N001",
                "Title" => "News 1",
                "Slug" => "news-1",
                "CreatedAt" => DateTime.Now,
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
        Assert.Single(result.Value!.Items);
        Assert.Equal("N001", result.Value.Items.First().Code);
    }
}
