using AutoMapper;
using Moq;
using VNVTStore.Application.Promotions.Commands;
using VNVTStore.Application.Promotions.Handlers;
using VNVTStore.Application.Promotions.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Tests.Promotions;

public class PromotionHandlersTests
{
    private readonly Mock<IRepository<TblPromotion>> _promoRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly PromotionHandlers _handler;

    public PromotionHandlersTests()
    {
        _promoRepoMock = new Mock<IRepository<TblPromotion>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _handler = new PromotionHandlers(
            _promoRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task CreatePromotion_ValidData_ReturnsSuccess()
    {
        // Arrange
        var promoDto = new PromotionDto { Code = "PROMO001", Name = "Summer Sale" };

        _promoRepoMock.Setup(r => r.AddAsync(It.IsAny<TblPromotion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<PromotionDto>(It.IsAny<TblPromotion>())).Returns(promoDto);

        // Act
        var result = await _handler.Handle(
            new CreatePromotionCommand("Summer Sale", "20% off", "percentage", 20, 100000, 500000, 
                DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 100),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Summer Sale", result.Value.Name);
    }

    [Fact]
    public async Task UpdatePromotion_NotFound_ReturnsFailure()
    {
        // Arrange
        var promoCode = "PROMO999";

        _promoRepoMock.Setup(r => r.GetByCodeAsync(promoCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblPromotion?)null);

        // Act
        var result = await _handler.Handle(
            new UpdatePromotionCommand(promoCode, "Updated Name", null, null, null, null, null, null, null, null),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeletePromotion_Success_SoftDeletes()
    {
        // Arrange
        var promoCode = "PROMO001";
        var promo = new TblPromotion { Code = promoCode, Name = "Test Promo", IsActive = true };

        _promoRepoMock.Setup(r => r.GetByCodeAsync(promoCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(promo);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new DeletePromotionCommand(promoCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(promo.IsActive); // Verify soft delete
    }

    [Fact]
    public async Task GetPromotionByCode_Exists_ReturnsPromotion()
    {
        // Arrange
        var promoCode = "PROMO001";
        var promo = new TblPromotion { Code = promoCode, Name = "Test Promo" };
        var promoDto = new PromotionDto { Code = promoCode, Name = "Test Promo" };

        _promoRepoMock.Setup(r => r.GetByCodeAsync(promoCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(promo);
        _mapperMock.Setup(m => m.Map<PromotionDto>(promo)).Returns(promoDto);

        // Act
        var result = await _handler.Handle(new GetPromotionByCodeQuery(promoCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test Promo", result.Value!.Name);
    }
}
