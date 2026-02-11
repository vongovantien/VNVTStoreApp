namespace VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Attributes;

public class ReviewDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string UserCode { get; set; } = null!;
    public string? OrderItemCode { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool? IsApproved { get; set; }
    
    [Reference("TblUser", "UserCode", "FullName")]
    public string? UserName { get; set; }
    
    [Reference("TblUser", "UserCode", "AvatarUrl")]
    public string? UserAvatar { get; set; }
    
    [Reference("TblProduct", "ProductCode", "Name")]
    public string? ProductName { get; set; }
    
    public string? ProductCode { get; set; }

    public string? ParentCode { get; set; }

    public List<ReviewDto> Replies { get; set; } = new();
}

public class CreateReviewDto
{
    public string UserCode { get; set; } = null!;
    public string? OrderItemCode { get; set; }
    public string? ProductCode { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
}

public class UpdateReviewDto
{
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool? IsApproved { get; set; }
}
