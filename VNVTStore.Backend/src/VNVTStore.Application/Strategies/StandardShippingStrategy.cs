using VNVTStore.Domain.Strategies;

namespace VNVTStore.Application.Strategies;

public class StandardShippingStrategy : IShippingStrategy
{
    private const decimal FreeShippingThreshold = 1000000; // 1 million VND
    private const decimal StandardRate = 30000; // 30k VND

    public decimal CalculateShippingFee(decimal orderTotal)
    {
        return orderTotal >= FreeShippingThreshold ? 0 : StandardRate;
    }
}
