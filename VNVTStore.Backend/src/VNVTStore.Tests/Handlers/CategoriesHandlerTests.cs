using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using System.Runtime.Serialization;
using VNVTStore.Application.Categories.Handlers;
using VNVTStore.Application.Categories.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using System.Linq.Expressions;

namespace VNVTStore.Tests.Handlers;

public class CategoriesHandlerTests
{
    private readonly Mock<IRepository<TblCategory>> _mockRepo;
    private readonly Mock<IRepository<TblProduct>> _mockProductRepo;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CategoriesHandler _handler;

    public CategoriesHandlerTests()
    {
        _mockRepo = new Mock<IRepository<TblCategory>>();
        _mockProductRepo = new Mock<IRepository<TblProduct>>();
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();

        _handler = new CategoriesHandler(
            _mockRepo.Object,
            _mockProductRepo.Object,
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
        _mockRepo.Verify(r => r.Delete(cat), Times.Once); // Hard delete as per handler logic for single delete? 
        // Wait, handler calls: DeleteAsync(..., softDelete: false)
    }

    [Fact]
    public async Task DeleteMultiple_ShouldFail_WhenActiveDependenciesFound()
    {
        // Setup blocked codes query
        var products = new List<TblProduct> 
        { 
             new TblProduct { CategoryCode = "CAT001", IsActive = true } 
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
        // We can't easily verify the count of DTOs without mocking Map properly with a list but 
        // we can verify logic or assume Mock returns empty.
        // Actually, BaseHandler calls ToListAsync on the filtered query.
        // MockQueryable should handle Where.
        
        // Ensure Mapper is called with the filtered list
        _mockMapper.Verify(m => m.Map<List<CategoryDto>>(It.Is<List<TblCategory>>(l => l.Count == 2)), Times.Once);
    }
}
