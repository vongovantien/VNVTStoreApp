using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using VNVTStore.Application.Categories.Handlers;
using VNVTStore.Application.Categories.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace VNVTStore.Tests.Handlers;

public class CategoriesHandlerTests
{
    private readonly Mock<IRepository<TblCategory>> _mockRepo;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDapperContext> _mockDapperContext;
    private readonly Mock<IFileService> _mockFileService;
    private readonly CategoryHandlers _handler;

    public CategoriesHandlerTests()
    {
        _mockRepo = new Mock<IRepository<TblCategory>>();
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockDapperContext = new Mock<IDapperContext>();
        _mockFileService = new Mock<IFileService>();

        _handler = new CategoryHandlers(
            _mockRepo.Object,
            _mockUow.Object,
            _mockMapper.Object,
            _mockDapperContext.Object,
            _mockFileService.Object
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
    public async Task Create_ShouldUploadImage_WhenProvided()
    {
        var command = new CreateCommand<CreateCategoryDto, CategoryDto>(new CreateCategoryDto 
        { 
            Name = "New Cat",
            ImageUrl = "http://example.com/image.png"
        });

        _mockMapper.Setup(m => m.Map<TblCategory>(It.IsAny<CreateCategoryDto>()))
            .Returns(new TblCategory { Name = "New Cat" });
        _mockMapper.Setup(m => m.Map<CategoryDto>(It.IsAny<TblCategory>()))
            .Returns(new CategoryDto { Name = "New Cat", Code = "CAT123" });

        _mockFileService.Setup(s => s.SaveAndLinkImagesAsync(It.IsAny<string>(), "TblCategory", It.IsAny<IEnumerable<string>>(), "categories", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<string>>(new List<string> { "http://example.com/image.png" }));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockFileService.Verify(s => s.SaveAndLinkImagesAsync(It.IsAny<string>(), "TblCategory", It.IsAny<IEnumerable<string>>(), "categories", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldSucceed_WhenCategoryExists()
    {
        var command = new UpdateCommand<UpdateCategoryDto, CategoryDto>("CAT001", new UpdateCategoryDto 
        { 
            Name = "Updated Cat"
        });

        var existingCat = new TblCategory { Code = "CAT001", Name = "Old Cat" };
        _mockRepo.Setup(r => r.GetByCodeAsync("CAT001", It.IsAny<CancellationToken>())).ReturnsAsync(existingCat);

        _mockMapper.Setup(m => m.Map<CategoryDto>(It.IsAny<TblCategory>()))
            .Returns(new CategoryDto { Name = "Updated Cat" });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldSucceed_WhenInactiveAndNoSubcategories()
    {
        var cat = new TblCategory { Code = "CAT001", IsActive = false, Name = "Test" };
        _mockRepo.Setup(r => r.GetByCodeAsync("CAT001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);

        var list = new List<TblCategory> { cat }.BuildMockDbSet();
        _mockRepo.Setup(r => r.AsQueryable()).Returns(list.Object);

        var command = new DeleteCommand<TblCategory>("CAT001");
        // DeleteAsync uses _repository.Update for soft delete
        _mockFileService.Setup(s => s.DeleteLinkedFilesAsync("CAT001", "TblCategory", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }
}
