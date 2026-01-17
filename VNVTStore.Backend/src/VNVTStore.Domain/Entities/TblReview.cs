using System;
using System.Collections.Generic;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblReview : IEntity
{
    public string Code { get; set; } = null!;

    public string? OrderItemCode { get; set; }

    public string UserCode { get; set; } = null!;

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsApproved { get; set; } // Keep original IsApproved logic separate if needed, or map to IsActive?
    
    public bool IsActive { get; set; } = true;

    public string? ModifiedType { get; set; }

    public virtual TblOrderItem? OrderItemCodeNavigation { get; set; }

    public virtual TblUser UserCodeNavigation { get; set; } = null!;
}
