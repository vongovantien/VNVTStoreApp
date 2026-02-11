namespace VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Attributes;

public class QuoteDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string UserCode { get; set; } = null!;
    [Reference("TblUser", "UserCode", "Username")]
    public string? UserName { get; set; }
    
    public decimal TotalAmount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    public string? Note { get; set; }
    public string? AdminNote { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    [ReferenceCollection(typeof(QuoteItemDto), "TblQuoteItem", "QuoteCode", "Code")]
    public List<QuoteItemDto> Items { get; set; } = new();
}

public class QuoteItemDto
{
    public string Code { get; set; } = null!;
    public string QuoteCode { get; set; } = null!;
    public string ProductCode { get; set; } = null!;
    [Reference("TblProduct", "ProductCode", "Name")]
    public string? ProductName { get; set; }
    
    public string? UnitCode { get; set; }
    [Reference("TblUnit", "UnitCode", "Name")]
    public string? UnitName { get; set; }
    
    public int Quantity { get; set; }
    public decimal RequestPrice { get; set; }
    public decimal ApprovedPrice { get; set; }
    public decimal TotalLineAmount { get; set; }
}

public class CreateQuoteDto
{
    public string? Note { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Company { get; set; }
    public List<CreateQuoteItemDto> Items { get; set; } = new();
}

public class CreateQuoteItemDto
{
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
    public string? UnitCode { get; set; }
    public decimal? RequestPrice { get; set; }
}

public class UpdateQuoteDto
{
    public string? Note { get; set; }
    public string? Status { get; set; }
    public string? AdminNote { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime? ExpiryDate { get; set; }
    // For modifying items, typically we use separate endpoints or full replace
    public List<UpdateQuoteItemDto>? Items { get; set; } 
}

public class UpdateQuoteItemDto 
{
    public string? Code { get; set; } // If null, new item
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal? ApprovedPrice { get; set; }
}
