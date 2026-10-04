namespace VNVTStore.Application.DTOs;

public class PaymentMethodDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsOnline { get; set; }
    public bool IsActive { get; set; }
    public bool IsFixed { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePaymentMethodDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsOnline { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public class UpdatePaymentMethodDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsOnline { get; set; }
    public bool? IsActive { get; set; }
}
