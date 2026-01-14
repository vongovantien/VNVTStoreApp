namespace VNVTStore.Domain.Strategies;

public interface IShippingStrategy
{
    decimal CalculateShippingFee(decimal orderTotal);
}
