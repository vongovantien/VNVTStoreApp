using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using VNVTStore.Application.Brands.Handlers;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Tests.Helpers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Application.Tests.Handlers;

public class BrandHandlersTests
{
    private readonly Mock<IRepository<TblBrand>> _brandRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDapperContext> _dapperContextMock;
    private readonly Mock<IFileService> _fileServiceMock;

    public BrandHandlersTests()
    {
        _brandRepositoryMock = new Mock<IRepository<TblBrand>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _dapperContextMock = new Mock<IDapperContext>();
        _fileServiceMock = new Mock<IFileService>();

        TestingUtils.SetupDapperMock(_dapperContextMock);
    }

    [Fact(Skip = "Dapper mocking issue in Unit Test environment")]
    public async Task Handle_GetBrands_ShouldReturnPagedList()
    {
        // Arrange
        var handler = new BrandHandlers(
            _brandRepositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _mapperMock.Object, 
            _dapperContextMock.Object,
            _fileServiceMock.Object);

        var query = new GetPagedQuery<BrandDto>(1, 10);

        // Act
        // Use reflection or direct call if handler is public and has GetPagedDapperAsync mocked via base?
        // Actually, Handler implements IRequestHandler<GetPagedQuery<BrandDto>, Result<PagedResult<BrandDto>>>
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_CreateBrand_Success_ShouldCallFileService()
    {
        // Arrange
        var dto = new CreateBrandDto { Name = "Brand A", LogoUrl = "data:image/png;base64,..." };
        var command = new CreateCommand<CreateBrandDto, BrandDto>(dto);
        
        var handler = new BrandHandlers(
            _brandRepositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _mapperMock.Object, 
            _dapperContextMock.Object,
            _fileServiceMock.Object);

        var brand = new TblBrand { Code = "BRN001", Name = "Brand A" };
        _mapperMock.Setup(m => m.Map<TblBrand>(It.IsAny<CreateBrandDto>())).Returns(brand);
        _fileServiceMock.Setup(f => f.SaveAndLinkImagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<string> { "path/to/logo.png" }.AsEnumerable()));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _fileServiceMock.Verify(f => f.SaveAndLinkImagesAsync(It.IsAny<string>(), "BRAND", It.IsAny<string[]>(), "brands", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(1));
    }
}
