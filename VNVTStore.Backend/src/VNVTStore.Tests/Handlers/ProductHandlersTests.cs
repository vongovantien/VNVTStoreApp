using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Products.Commands;
using VNVTStore.Application.Products.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Tests.Handlers;

public class ProductHandlersTests
{
    private readonly Mock<IRepository<TblProduct>> _mockRepo;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IImageUploadService> _mockUploadService;
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly ProductHandlers _handler;

    public ProductHandlersTests()
    {
        _mockRepo = new Mock<IRepository<TblProduct>>();
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockUploadService = new Mock<IImageUploadService>();
        _mockContext = new Mock<IApplicationDbContext>();

        _handler = new ProductHandlers(
            _mockRepo.Object,
            _mockUow.Object,
            _mockMapper.Object,
            _mockUploadService.Object,
            _mockContext.Object
        );
    }

    [Fact]
    public async Task Create_ShouldAddProduct_WhenValid()
    {
        // Arrange
        var createDto = new CreateProductDto 
        { 
            Name = "New Product", 
            Price = 100, 
            StockQuantity = 10 
        };
        var command = new CreateCommand<CreateProductDto, ProductDto>(createDto);

        // Remove mapping to TblProduct setup as Handle uses TblProduct.Create
        var productDto = new ProductDto { Name = "New Product", Code = "P001" };

        _mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<TblProduct>()))
            .Returns(productDto);

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<TblProduct>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Mock empty files for simplicity in this basic test
        var files = new List<TblFile>().AsQueryable().BuildMock();
        _mockContext.Setup(c => c.TblFiles).Returns(files.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<TblProduct>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldUploadImages_WhenImagesProvided()
    {
        // Arrange
        var base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";
        var createDto = new CreateProductDto 
        { 
            Name = "Product With Image", 
            Images = new List<string> { base64Image } 
        };
        var command = new CreateCommand<CreateProductDto, ProductDto>(createDto);
        
        _mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<TblProduct>())).Returns(new ProductDto());

        // Mock Upload Service
        var uploadedPath = "products/newimage.png";
        _mockUploadService.Setup(s => s.UploadBase64ImagesAsync(It.IsAny<List<(string, string)>>(), "products"))
            .ReturnsAsync(Result.Success<IEnumerable<VNVTStore.Application.Common.Models.FileDto>>(new List<VNVTStore.Application.Common.Models.FileDto> { new VNVTStore.Application.Common.Models.FileDto { Path = uploadedPath, Url = uploadedPath } }));

        // Mock TblFiles database to find the file after "upload" (simulated logic needing Query)
        // In the real handler, it queries TblFiles by Path. We need to mock that TblFiles contains the file.
        var fileEntity = TblFile.Create("newimage.png", "newimage.png", ".png", "image/png", 1024, uploadedPath);
        var files = new List<TblFile> { fileEntity }.AsQueryable().BuildMock();
        _mockContext.Setup(c => c.TblFiles).Returns(files.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUploadService.Verify(s => s.UploadBase64ImagesAsync(It.IsAny<List<(string, string)>>(), "products"), Times.Once);
        
        // Verify linking happened on the file entity itself
        Assert.Equal("P001", fileEntity.MasterCode);
        Assert.Equal("Product", fileEntity.MasterType);

        _mockRepo.Verify(r => r.AddAsync(It.Is<TblProduct>(p => p.Name == "Product With Image"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
