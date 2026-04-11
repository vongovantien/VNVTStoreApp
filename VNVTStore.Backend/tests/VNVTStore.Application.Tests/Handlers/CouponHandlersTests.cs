using AutoMapper;
using MediatR;
using Moq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VNVTStore.Application.Common;
using VNVTStore.Application.Coupons.Commands;
using VNVTStore.Application.Coupons.Handlers;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Application.Tests.Handlers;

public class CouponHandlersTests
{
    private readonly Mock<IRepository<TblCoupon>> _couponRepositoryMock;
    private readonly Mock<IRepository<TblPromotion>> _promotionRepositoryMock;
    private readonly Mock<ICouponService> _couponServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDapperContext> _dapperContextMock;
    private readonly CouponHandlers _handler;

    public CouponHandlersTests()
    {
        _couponRepositoryMock = new Mock<IRepository<TblCoupon>>();
        _promotionRepositoryMock = new Mock<IRepository<TblPromotion>>();
        _couponServiceMock = new Mock<ICouponService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _dapperContextMock = new Mock<IDapperContext>();

        _handler = new CouponHandlers(
            _couponRepositoryMock.Object,
            _promotionRepositoryMock.Object,
            _couponServiceMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _dapperContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_CreateCoupon_Success_ShouldReturnCouponDto()
    {
        // Arrange
        var dto = new CreateCouponDto
        {
            PromotionCode = "PROMO001"
        };
        var command = new CreateCommand<CreateCouponDto, CouponDto>(dto);

        _promotionRepositoryMock.Setup(x => x.GetByCodeAsync("PROMO001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TblPromotion { Code = "PROMO001" });

        _mapperMock.Setup(x => x.Map<TblCoupon>(dto)).Returns(new TblCoupon());
        _mapperMock.Setup(x => x.Map<CouponDto>(It.IsAny<TblCoupon>())).Returns(new CouponDto { Code = "COUPON_GENERATED" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("COUPON_GENERATED", result.Value!.Code);
        _couponRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TblCoupon>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateCoupon_InvalidPromotion_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new CreateCouponDto { PromotionCode = "INVALID" };
        var command = new CreateCommand<CreateCouponDto, CouponDto>(dto);

        _promotionRepositoryMock.Setup(x => x.GetByCodeAsync("INVALID", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblPromotion?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ValidateCoupon_ShouldCallService()
    {
        // Arrange
        var command = new ValidateCouponCommand("COUPON123", 1000);
        var expectedDto = new CouponDto { Code = "COUPON123" };
        
        _couponServiceMock.Setup(x => x.ValidateCouponAsync("COUPON123", 1000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedDto));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("COUPON123", result.Value!.Code);
        _couponServiceMock.Verify(x => x.ValidateCouponAsync("COUPON123", 1000, It.IsAny<CancellationToken>()), Times.Once);
    }
}
