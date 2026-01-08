using AutoMapper;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.Coupons.Commands;
using VNVTStore.Application.Coupons.Handlers;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Tests.Coupons;

public class CouponHandlersTests
{
    private readonly Mock<IRepository<TblCoupon>> _couponRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CouponHandlers _handler;

    public CouponHandlersTests()
    {
        _couponRepoMock = new Mock<IRepository<TblCoupon>>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CouponHandlers(_couponRepoMock.Object, _mapperMock.Object);
    }

    /*
     * Warning: This test checks complex validation logic involving Includes.
     * Mocking Include chains directly with Moq/IRepository IQueryable is brittle.
     * ideally we use an in-memory DB or a robust mock helper.
     * For now, we simulate the result of the query.
     */
}
