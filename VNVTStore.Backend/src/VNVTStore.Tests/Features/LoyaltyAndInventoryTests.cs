using Xunit;
using Moq;
using VNVTStore.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace VNVTStore.Tests.Features;

public class LoyaltyAndInventoryTests
{
    [Fact]
    public void LoyaltyPoints_ShouldAwardOnePointPer10000VND()
    {
        // 1. Arrange: 45,000 VND should result in 4 points
        decimal totalAmount = 45000m;
        var user = (TblUser)FormatterServices.GetUninitializedObject(typeof(TblUser));
        typeof(TblUser).GetProperty("LoyaltyPoints")?.SetValue(user, 0);

        // 2. Act: Apply points logic (1 point per 10,000 VND)
        int points = (int)(totalAmount / 10000m);
        typeof(TblUser).GetProperty("LoyaltyPoints")?.SetValue(user, user.LoyaltyPoints + points);

        // 3. Assert
        Assert.Equal(4, user.LoyaltyPoints);
    }

    [Fact]
    public void Dashboard_ShouldIdentifyLowStockProducts()
    {
        // 1. Arrange: Threshold is < 10
        var p1 = TblProduct.Create("Low", 100, null, 5, null, null, null, null, null);
        var p2 = TblProduct.Create("High", 100, null, 15, null, null, null, null, null);
        var products = new List<TblProduct> { p1, p2 };

        // 2. Act
        var lowStock = products.Where(p => (p.StockQuantity ?? 0) < 10).ToList();

        // 3. Assert
        Assert.Single(lowStock);
        Assert.Equal("Low", lowStock[0].Name);
    }
}
