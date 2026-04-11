using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using VNVTStore.Application.Banners.Handlers;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Tests.Helpers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Application.Tests.Handlers;

public class BannerHandlersTests
{
    private readonly Mock<IRepository<TblBanner>> _bannerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDapperContext> _dapperContextMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IBaseUrlService> _baseUrlServiceMock;

    public BannerHandlersTests()
    {
        _bannerRepositoryMock = new Mock<IRepository<TblBanner>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _dapperContextMock = new Mock<IDapperContext>();
        _fileServiceMock = new Mock<IFileService>();
        _baseUrlServiceMock = new Mock<IBaseUrlService>();

        TestingUtils.SetupDapperMock(_dapperContextMock);
    }

    [Fact]
    public async Task Handle_CreateBanner_Success_ShouldReturnBannerDto()
    {
        // Arrange
        var dto = new CreateBannerDto { Title = "Summer Sale", ImageURL = "data:image/png;base64,..." };
        var command = new CreateCommand<CreateBannerDto, BannerDto>(dto);
        
        var handler = new CreateBannerHandler(
            _bannerRepositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _mapperMock.Object, 
            _dapperContextMock.Object,
            _fileServiceMock.Object,
            _baseUrlServiceMock.Object);

        var banner = new TblBanner { Code = "BNN001", Title = "Summer Sale" };
        _mapperMock.Setup(m => m.Map<TblBanner>(It.IsAny<CreateBannerDto>())).Returns(banner);
        _mapperMock.Setup(m => m.Map<BannerDto>(It.IsAny<TblBanner>())).Returns(new BannerDto { Code = "BNN001", Title = "Summer Sale" });
        
        _fileServiceMock.Setup(f => f.SaveAndLinkImagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<string> { "uploads/banners/sale.png" }.AsEnumerable()));
        
        _baseUrlServiceMock.Setup(b => b.GetBaseUrl()).Returns("http://localhost:5000");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Summer Sale", result.Value!.Title);
        _fileServiceMock.Verify(x => x.SaveAndLinkImagesAsync(It.IsAny<string>(), "Banner", It.IsAny<string[]>(), "banners", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(Skip = "Dapper mocking issue in Unit Test environment")]
    public async Task Handle_GetBanners_ShouldWorkWithDapper()
    {
        // Arrange
        var query = new GetPagedQuery<BannerDto>(1, 10);
        var handler = new GetBannersHandler(
            _bannerRepositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _mapperMock.Object, 
            _dapperContextMock.Object,
            _baseUrlServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
