using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using System.Runtime.Serialization;
using VNVTStore.Application.Categories.Handlers;
using VNVTStore.Application.Categories.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using System.Linq.Expressions;

namespace VNVTStore.Tests.Handlers;

public class CategoriesHandlerTests
{
    private readonly Mock<IRepository<TblCategory>> _mockRepo;
    private readonly Mock<IRepository<TblProduct>> _mockProductRepo;
    private readonly Mock<IImageUploadService> _mockImageUploadService;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CategoriesHandler _handler;

    public CategoriesHandlerTests()
    {
        _mockRepo = new Mock<IRepository<TblCategory>>();
        _mockProductRepo = new Mock<IRepository<TblProduct>>();
        _mockImageUploadService = new Mock<IImageUploadService>();
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockContext = new Mock<IApplicationDbContext>();

        // Setup default TblFiles mock to return empty list to avoid null ref
        var files = new List<TblFile>().AsQueryable().BuildMock();
        _mockContext.Setup(c => c.TblFiles).Returns(files.Object);

        _handler = new CategoriesHandler(
            _mockRepo.Object,
            _mockProductRepo.Object,
            _mockImageUploadService.Object,
            _mockContext.Object,
            _mockUow.Object,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task Create_ShouldCallAdd_WhenValid()
    {
        var command = new CreateCommand<CreateCategoryDto, CategoryDto>(new CreateCategoryDto { Name = "New Cat" });
        _mockMapper.Setup(m => m.Map<TblCategory>(It.IsAny<CreateCategoryDto>()))
            .Returns(new TblCategory { Name = "New Cat" });
        _mockMapper.Setup(m => m.Map<CategoryDto>(It.IsAny<TblCategory>()))
            .Returns(new CategoryDto { Name = "New Cat" });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<TblCategory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldUploadImage_WhenBase64Provided()
    {
        var base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKwAEQAAAABJRU5ErkJggg==";
        var command = new CreateCommand<CreateCategoryDto, CategoryDto>(new CreateCategoryDto 
        { 
            Name = "New Cat",
            ImageUrl = base64Image
        });

        _mockImageUploadService.Setup(s => s.UploadBase64Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success(new VNVTStore.Application.Common.Models.FileDto { Path = "http://example.com/image.png", Url = "http://example.com/image.png" }));

        _mockMapper.Setup(m => m.Map<TblCategory>(It.IsAny<CreateCategoryDto>()))
            .Returns(new TblCategory { Name = "New Cat", ImageUrl = "http://example.com/image.png" });
        var createdDto = new CategoryDto { Name = "New Cat", ImageUrl = "http://example.com/image.png", Code = "CAT123" };
        _mockMapper.Setup(m => m.Map<CategoryDto>(It.IsAny<TblCategory>()))
            .Returns(createdDto);

        // Mock TblFile existence for linking logic
        var fileEntity = TblFile.Create("image.png", "image.png", ".png", "image/png", 100, "http://example.com/image.png");
        var files = new List<TblFile> { fileEntity }.AsQueryable().BuildMock();
        _mockContext.Setup(c => c.TblFiles).Returns(files.Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockImageUploadService.Verify(s => s.UploadBase64Async(base64Image, It.IsAny<string>(), "categories"), Times.Once);
        Assert.Equal("http://example.com/image.png", command.Dto.ImageUrl);
        
        // Verify linking happened
        Assert.Equal("CAT123", fileEntity.MasterCode);
        Assert.Equal("Category", fileEntity.MasterType);
    }

    [Fact]
    public async Task Update_ShouldUploadImage_WhenBase64Provided()
    {
        var base64Image = "data:image/png;base64,test";
        var command = new UpdateCommand<UpdateCategoryDto, CategoryDto>("CAT001", new UpdateCategoryDto 
        { 
            Name = "Updated Cat",
            ImageUrl = base64Image
        });

        var existingCat = new TblCategory { Code = "CAT001", Name = "Old Cat" };
        _mockRepo.Setup(r => r.GetByCodeAsync("CAT001", It.IsAny<CancellationToken>())).ReturnsAsync(existingCat);

        _mockImageUploadService.Setup(s => s.UploadBase64Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success(new VNVTStore.Application.Common.Models.FileDto { Path = "http://example.com/new-image.png", Url = "http://example.com/new-image.png" }));

        _mockMapper.Setup(m => m.Map(It.IsAny<UpdateCategoryDto>(), It.IsAny<TblCategory>()))
            .Callback<UpdateCategoryDto, TblCategory>((dto, entity) => entity.ImageUrl = dto.ImageUrl);
            
        _mockMapper.Setup(m => m.Map<CategoryDto>(It.IsAny<TblCategory>()))
            .Returns(new CategoryDto { Name = "Updated Cat", ImageUrl = "http://example.com/new-image.png" });

        // Mock files for update logic
        var oldFile = TblFile.Create("old.png", "old.png", ".png", "image/png", 100, "http://example.com/old.png");
        oldFile.MasterCode = "CAT001";
        oldFile.MasterType = "Category";

        var newFile = TblFile.Create("new.png", "new.png", ".png", "image/png", 100, "http://example.com/new-image.png");
        
        var filesList = new List<TblFile> { oldFile, newFile };
        var filesMock = filesList.AsQueryable().BuildMock();
        _mockContext.Setup(c => c.TblFiles).Returns(filesMock.Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockImageUploadService.Verify(s => s.UploadBase64Async(base64Image, It.IsAny<string>(), "categories"), Times.Once);
        Assert.Equal("http://example.com/new-image.png", command.Dto.ImageUrl);

        // Verify unlinking and linking
        Assert.Null(oldFile.MasterCode); // Should be unlinked
        Assert.Equal("CAT001", newFile.MasterCode);
        Assert.Equal("Category", newFile.MasterType);
    }

    [Fact]
    public async Task Delete_ShouldFail_WhenHasProducts()
    {
        // Mock products existing
        _mockProductRepo.Setup(r => r.CountAsync(It.IsAny<Expression<Func<TblProduct, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5); // 5 products

        var command = new DeleteCommand<TblCategory>("CAT001");
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("products", result.Error.Message);
    }

    [Fact]
    public async Task Delete_ShouldSucceed_WhenNoProducts()
    {
        _mockProductRepo.Setup(r => r.CountAsync(It.IsAny<Expression<Func<TblProduct, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var cat = new TblCategory { Code = "CAT001", IsActive = false, Name="Test" };
        _mockRepo.Setup(r => r.GetByCodeAsync("CAT001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);

        var command = new DeleteCommand<TblCategory>("CAT001");
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockRepo.Verify(r => r.Delete(cat), Times.Once); 
    }

    [Fact]
    public async Task DeleteMultiple_ShouldFail_WhenActiveDependenciesFound()
    {
        // Setup blocked codes query
        var products = new List<TblProduct> 
        { 
             TblProduct.Create("Product", 10, 10, "CAT001", "SKU001", 10, 1, "SUP", "Red", "100W", "220V", "Plastic", "M")
        }.AsQueryable().BuildMock();

        _mockProductRepo.Setup(r => r.AsQueryable()).Returns(products);

        // Also mock Categories repo for the blocking check to find names
        var cats = new List<TblCategory> { new TblCategory { Code = "CAT001", Name = "Blocked Cat" } };
        _mockRepo.Setup(r => r.AsQueryable()).Returns(cats.AsQueryable().BuildMock());

        var command = new DeleteMultipleCommand<TblCategory>(new List<string> { "CAT001" });
        
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Blocked Cat", result.Error.Message);
    }

    [Fact]
    public async Task DeleteMultiple_ShouldSucceed_WhenNoDependenciesAndInactive()
    {
        // Setup no active products
        var products = new List<TblProduct>().AsQueryable().BuildMock();
        _mockProductRepo.Setup(r => r.AsQueryable()).Returns(products);

        // Setup Inactive Categories
        var cats = new List<TblCategory> { new TblCategory { Code = "CAT001", IsActive = false } };
        _mockRepo.Setup(r => r.AsQueryable()).Returns(cats.AsQueryable().BuildMock());

        var command = new DeleteMultipleCommand<TblCategory>(new List<string> { "CAT001" });
        
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockRepo.Verify(r => r.Update(It.IsAny<TblCategory>()), Times.Once); // Soft delete
    }

    [Fact]
    public async Task GetPaged_ShouldFilterSoftDeleted()
    {
        var list = new List<TblCategory>
        {
            new TblCategory { Code="C1", ModifiedType = null, Name="A" },
            new TblCategory { Code="C2", ModifiedType = "Delete", Name="B" }, // Should be excluded
            new TblCategory { Code="C3", ModifiedType = "Update", Name="C" }
        }.AsQueryable().BuildMock();

        _mockRepo.Setup(r => r.AsQueryable()).Returns(list);

        var query = new GetPagedQuery<CategoryDto> { PageIndex = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        
        // Ensure Mapper is called with the filtered list
        _mockMapper.Verify(m => m.Map<List<CategoryDto>>(It.Is<List<TblCategory>>(l => l.Count == 2)), Times.Once);
    }
}
