using AutoMapper;
using Moq;
using Xunit;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Promotions.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Application.Tests.Handlers;

public class PromotionHandlersTests
{
    private readonly Mock<IRepository<TblPromotion>> _promotionRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly PromotionHandlers _handler;
    private readonly List<TblPromotion> _promotionsDatabase;

    public PromotionHandlersTests()
    {
        _promotionRepoMock = new Mock<IRepository<TblPromotion>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _promotionsDatabase = new List<TblPromotion>();

        _promotionRepoMock.Setup(x => x.AddAsync(It.IsAny<TblPromotion>(), It.IsAny<CancellationToken>()))
            .Callback<TblPromotion, CancellationToken>((p, _) => _promotionsDatabase.Add(p))
            .Returns(Task.CompletedTask);

        _promotionRepoMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string code, CancellationToken _) => _promotionsDatabase.FirstOrDefault(p => p.Code == code));

        _handler = new PromotionHandlers(_promotionRepoMock.Object, _unitOfWorkMock.Object, _mapperMock.Object, new Mock<IDapperContext>().Object);
    }

    [Fact]
    public async Task Handle_CreatePromotion_ShouldSucceed()
    {
        // Arrange
        var createDto = new CreatePromotionDto
        {
            Name = "Winter Sale",
            DiscountType = "PERCENTAGE",
            DiscountValue = 20,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };

        var entity = new TblPromotion { Name = createDto.Name, DiscountType = createDto.DiscountType, DiscountValue = createDto.DiscountValue };
        _mapperMock.Setup(x => x.Map<TblPromotion>(createDto)).Returns(entity);
        _mapperMock.Setup(x => x.Map<PromotionDto>(It.IsAny<TblPromotion>())).Returns(new PromotionDto { Name = createDto.Name });

        var command = new CreateCommand<CreatePromotionDto, PromotionDto>(createDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(_promotionsDatabase);
        Assert.Equal("Winter Sale", _promotionsDatabase[0].Name);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CreatePromotion_WithProductCodes_ShouldLinkProducts()
    {
        // Arrange
        var createDto = new CreatePromotionDto
        {
            Name = "Flash Sale",
            ProductCodes = new List<string> { "PROD001", "PROD002" }
        };

        var entity = new TblPromotion { Name = createDto.Name };
        _mapperMock.Setup(x => x.Map<TblPromotion>(createDto)).Returns(entity);
        _mapperMock.Setup(x => x.Map<PromotionDto>(It.IsAny<TblPromotion>())).Returns(new PromotionDto { Name = createDto.Name });

        var command = new CreateCommand<CreatePromotionDto, PromotionDto>(createDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, _promotionsDatabase[0].TblProductPromotions.Count);
        Assert.Contains(_promotionsDatabase[0].TblProductPromotions, p => p.ProductCode == "PROD001");
        Assert.Contains(_promotionsDatabase[0].TblProductPromotions, p => p.ProductCode == "PROD002");
    }

    [Fact]
    public async Task Handle_UpdatePromotion_ShouldUpdateEntity()
    {
        // Arrange
        var existingPromotion = new TblPromotion { Code = "SALE20", Name = "Old Sale" };
        _promotionsDatabase.Add(existingPromotion);

        var updateDto = new UpdatePromotionDto
        {
            Name = "New Sale",
            IsActive = false
        };

        _mapperMock.Setup(x => x.Map(updateDto, existingPromotion)).Callback(() => {
            existingPromotion.Name = updateDto.Name;
            existingPromotion.IsActive = updateDto.IsActive;
        });
        _mapperMock.Setup(x => x.Map<PromotionDto>(existingPromotion)).Returns(new PromotionDto { Name = updateDto.Name });

        var command = new UpdateCommand<UpdatePromotionDto, PromotionDto>("SALE20", updateDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Sale", existingPromotion.Name);
        Assert.False(existingPromotion.IsActive);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdatePromotion_NotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateCommand<UpdatePromotionDto, PromotionDto>("NONEXISTENT", new UpdatePromotionDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }
}
