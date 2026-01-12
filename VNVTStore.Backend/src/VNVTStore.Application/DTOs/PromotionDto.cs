using System;
using System.Collections.Generic;

namespace VNVTStore.Application.DTOs;

public class PromotionDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = null!; // "PERCENTAGE", "AMOUNT"
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public bool? IsActive { get; set; }
    
    // For Flash Sale (list of product codes)
    public List<string>? ProductCodes { get; set; }
}

public class CreatePromotionDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "AMOUNT";
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public bool IsActive { get; set; } = true;
    
    public List<string>? ProductCodes { get; set; }
}

public class UpdatePromotionDto : CreatePromotionDto
{
}
