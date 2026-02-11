namespace VNVTStore.Application.DTOs;

public class CouponDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string? PromotionCode { get; set; }
    public int? UsageCount { get; set; }
    public bool IsValid { get; set; }
}

public class CreateCouponDto
{
    public string? PromotionCode { get; set; }
}
