using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using VNVTStore.Application.DTOs;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace VNVTStore.Tests.Common;

public class TestEntity : TblCategory // Using TblCategory as concrete implementation for abstract BaseHandler
{
    // Wrapper for testing protected methods
}

public class TestHandler : BaseHandler<TblCategory>
{
    public TestHandler(
        IRepository<TblCategory> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(repository, unitOfWork, mapper)
    {
    }

    public Task<Result> TestCheckBlockingDependenciesAsync(
        IQueryable<string> blockingEntityCodesQuery,
        string dependencyEntityName,
        CancellationToken cancellationToken)
    {
        return CheckBlockingDependenciesAsync(blockingEntityCodesQuery, dependencyEntityName, cancellationToken);
    }

    public Task<Result> TestDeleteAsync(
        string code,
        string entityName,
        CancellationToken cancellationToken,
        bool softDelete = true)
    {
        return DeleteAsync(code, entityName, cancellationToken, softDelete);
    }

    public Task<Result> TestDeleteMultipleAsync(
        List<string> codes,
        string entityName,
        CancellationToken cancellationToken)
    {
        return DeleteMultipleAsync(codes, entityName, cancellationToken);
    }
}

public class BaseHandlerTests
{
    private readonly Mock<IRepository<TblCategory>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly TestHandler _handler;

    public BaseHandlerTests()
    {
        _mockRepository = new Mock<IRepository<TblCategory>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _handler = new TestHandler(_mockRepository.Object, _mockUnitOfWork.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CheckBlockingDependenciesAsync_ShouldReturnSuccess_WhenNoDependenciesBlocking()
    {
        // Arrange
        var blockingCodes = new List<string>().AsQueryable().BuildMock();

        // Act
        var result = await _handler.TestCheckBlockingDependenciesAsync(blockingCodes, "products", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CheckBlockingDependenciesAsync_ShouldReturnFailure_WhenDependenciesBlocking()
    {
        // Arrange
        var blockingCodes = new List<string> { "CAT001" }.AsQueryable().BuildMock();

        var categories = new List<TblCategory>();
        var cat = (TblCategory)FormatterServices.GetUninitializedObject(typeof(TblCategory));
        cat.Code = "CAT001";
        cat.Name = "Test Category";
        categories.Add(cat);

        var mockDbSet = categories.AsQueryable().BuildMock();
        _mockRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet);

        // Act
        var result = await _handler.TestCheckBlockingDependenciesAsync(blockingCodes, "products", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Test Category", result.Error.Message);
    }

    [Fact]
    public async Task CheckBlockingDependenciesAsync_ShouldReturnSuccess_WhenDependenciesInactive()
    {
        // Arrange
        var blockingCodes = new List<string>().AsQueryable().BuildMock();

        // Act
        var result = await _handler.TestCheckBlockingDependenciesAsync(blockingCodes, "products", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    // --- DeleteAsync Tests ---

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenEntityNotFound()
    {
        _mockRepository.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblCategory?)null);

        var result = await _handler.TestDeleteAsync("UNKNOWN", "Category", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(Error.NotFound("Category", "UNKNOWN"), result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenEntityIsActive()
    {
        var cat = (TblCategory)FormatterServices.GetUninitializedObject(typeof(TblCategory));
        cat.Code = "CAT001";
        cat.IsActive = true;

        _mockRepository.Setup(r => r.GetByCodeAsync("CAT001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);

        var result = await _handler.TestDeleteAsync("CAT001", "Category", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("active", result.Error.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenInactiveAndSoftDeleteTrue()
    {
        var cat = (TblCategory)FormatterServices.GetUninitializedObject(typeof(TblCategory));
        cat.Code = "CAT001";
        cat.IsActive = false; // Inactive, allowed to delete
        cat.ModifiedType = "Update";

        _mockRepository.Setup(r => r.GetByCodeAsync("CAT001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);

        // Setup unit of work
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.TestDeleteAsync("CAT001", "Category", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(cat.IsActive);
        Assert.Equal("Delete", cat.ModifiedType);
        _mockRepository.Verify(r => r.Update(cat), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteAsync_ShouldHardDelete_WhenSoftDeleteFalse()
    {
        var cat = (TblCategory)FormatterServices.GetUninitializedObject(typeof(TblCategory));
        cat.Code = "CAT001";
        cat.IsActive = false;

        _mockRepository.Setup(r => r.GetByCodeAsync("CAT001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cat);
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.TestDeleteAsync("CAT001", "Category", CancellationToken.None, softDelete: false);

        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.Delete(cat), Times.Once);
    }

    // --- DeleteMultipleAsync Tests ---

    [Fact]
    public async Task DeleteMultipleAsync_ShouldReturnFailure_WhenAnyActive()
    {
        var cat1 = new TblCategory { Code = "C1", IsActive = false };
        var cat2 = new TblCategory { Code = "C2", IsActive = true }; // Active!
        var list = new List<TblCategory> { cat1, cat2 };

        _mockRepository.Setup(r => r.AsQueryable()).Returns(list.AsQueryable().BuildMock());

        var result = await _handler.TestDeleteMultipleAsync(new List<string> { "C1", "C2" }, "Category", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("C2", result.Error.Message);
        _mockRepository.Verify(r => r.Update(It.IsAny<TblCategory>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMultipleAsync_ShouldSoftDeleteAll_WhenAllInactive()
    {
        var cat1 = new TblCategory { Code = "C1", IsActive = false };
        var cat2 = new TblCategory { Code = "C2", IsActive = false };
        var list = new List<TblCategory> { cat1, cat2 };

        _mockRepository.Setup(r => r.AsQueryable()).Returns(list.AsQueryable().BuildMock());
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.TestDeleteMultipleAsync(new List<string> { "C1", "C2" }, "Category", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Delete", cat1.ModifiedType);
        Assert.Equal("Delete", cat2.ModifiedType);
        _mockRepository.Verify(r => r.Update(It.IsAny<TblCategory>()), Times.Exactly(2));
    }
}
