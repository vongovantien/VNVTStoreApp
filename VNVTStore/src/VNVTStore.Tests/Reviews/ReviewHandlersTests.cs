using AutoMapper;
using Moq;
using VNVTStore.Application.Reviews.Commands;
using VNVTStore.Application.Reviews.Handlers;
using VNVTStore.Application.Reviews.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Tests.Extensions;
using Xunit;

namespace VNVTStore.Tests.Reviews;

public class ReviewHandlersTests
{
    private readonly Mock<IRepository<TblReview>> _reviewRepoMock;
    private readonly Mock<IRepository<TblOrderItem>> _orderItemRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ReviewHandlers _handler;

    public ReviewHandlersTests()
    {
        _reviewRepoMock = new Mock<IRepository<TblReview>>();
        _orderItemRepoMock = new Mock<IRepository<TblOrderItem>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _handler = new ReviewHandlers(
            _reviewRepoMock.Object,
            _orderItemRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task CreateReview_Valid_ReturnsSuccess()
    {
        // Arrange
        var userCode = "USR001";
        var orderItemCode = "OI001";
        var orderCode = "ORD001";
        
        var orderItem = new TblOrderItem 
        { 
            Code = orderItemCode, 
            OrderCode = orderCode,
            OrderCodeNavigation = new TblOrder { UserCode = userCode } 
        };

        var orderItems = new List<TblOrderItem> { orderItem }.BuildMock();
        
        _orderItemRepoMock.Setup(r => r.AsQueryable()).Returns(orderItems);
        
        _reviewRepoMock.Setup(r => r.FindAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<TblReview, bool>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblReview?)null);

        _reviewRepoMock.Setup(r => r.AddAsync(It.IsAny<TblReview>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var reviewDto = new ReviewDto { Code = "NewReview" };
        _mapperMock.Setup(m => m.Map<ReviewDto>(It.IsAny<TblReview>())).Returns(reviewDto);

        // Act
        var result = await _handler.Handle(
            new CreateReviewCommand(userCode, orderItemCode, 5, "Title", "Content"), 
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateReview_NotFound_ReturnsFailure()
    {
        // Arrange
        var reviewCode = "REV999";
        var userCode = "USR001";

        _reviewRepoMock.Setup(r => r.GetByCodeAsync(reviewCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblReview?)null);

        // Act
        var result = await _handler.Handle(
            new UpdateReviewCommand(reviewCode, userCode, 5, null, "Updated content"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateReview_NotOwner_ReturnsForbidden()
    {
        // Arrange
        var reviewCode = "REV001";
        var userCode = "USR001";
        var review = new TblReview { Code = reviewCode, UserCode = "USR002", Rating = 3 };

        _reviewRepoMock.Setup(r => r.GetByCodeAsync(reviewCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var result = await _handler.Handle(
            new UpdateReviewCommand(reviewCode, userCode, 5, null, "Updated"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Forbidden", result.Error!.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateReview_Owner_ReturnsSuccess()
    {
        // Arrange
        var reviewCode = "REV001";
        var userCode = "USR001";
        var review = new TblReview { Code = reviewCode, UserCode = userCode, Rating = 3, Comment = "Old" };
        var reviewDto = new ReviewDto { Code = reviewCode, Rating = 5, Comment = "Updated" };

        _reviewRepoMock.Setup(r => r.GetByCodeAsync(reviewCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<ReviewDto>(It.IsAny<TblReview>())).Returns(reviewDto);

        // Act
        var result = await _handler.Handle(
            new UpdateReviewCommand(reviewCode, userCode, 5, null, "Updated"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, review.Rating);
        Assert.Equal("Updated", review.Comment);
    }

    [Fact]
    public async Task DeleteReview_NotOwner_ReturnsForbidden()
    {
        // Arrange
        var reviewCode = "REV001";
        var userCode = "USR001";
        var review = new TblReview { Code = reviewCode, UserCode = "USR002" };

        _reviewRepoMock.Setup(r => r.GetByCodeAsync(reviewCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var result = await _handler.Handle(
            new DeleteReviewCommand(reviewCode, userCode),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Forbidden", result.Error!.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteReview_Owner_ReturnsSuccess()
    {
        // Arrange
        var reviewCode = "REV001";
        var userCode = "USR001";
        var review = new TblReview { Code = reviewCode, UserCode = userCode };

        _reviewRepoMock.Setup(r => r.GetByCodeAsync(reviewCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(
            new DeleteReviewCommand(reviewCode, userCode),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        _reviewRepoMock.Verify(r => r.Delete(review), Times.Once);
    }
}
