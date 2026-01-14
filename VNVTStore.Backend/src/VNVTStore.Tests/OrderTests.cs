using Xunit;
using VNVTStore.Domain.Entities;
using System;

namespace VNVTStore.Tests;

public class OrderTests
{
    [Fact]
    public void CreateOrder_ShouldInitializeCorrectly()
    {
        // Arrange
        var userCode = "USER001";
        var addressCode = "ADDR001";
        var totalAmount = 1000m;
        var shippingFee = 50m;
        var discount = 10m;
        var couponCode = "SAVE10";

        // Act
        var order = TblOrder.Create(
            userCode,
            addressCode,
            totalAmount,
            shippingFee,
            discount,
            couponCode
        );

        // Assert
        Assert.NotNull(order);
        Assert.Equal(userCode, order.UserCode);
        Assert.Equal(addressCode, order.AddressCode);
        Assert.Equal(totalAmount, order.TotalAmount);
        Assert.Equal("Pending", order.Status); // Assuming default status
        Assert.Equal(couponCode, order.CouponCode);
    }

    [Fact]
    public void AddOrderItem_ShouldAddItemToCollection()
    {
        // Arrange
        var order = TblOrder.Create("U1", "A1", 100, 0, 0, null);
        var item = TblOrderItem.Create("P1", 2, 50, "M", "Red");

        // Act
        order.AddOrderItem(item);

        // Assert
        Assert.Single(order.TblOrderItems);
        Assert.Equal("P1", order.TblOrderItems.First().ProductCode);
    }
}
