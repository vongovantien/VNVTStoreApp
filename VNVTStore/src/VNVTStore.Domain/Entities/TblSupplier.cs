using System;
using System.Collections.Generic;

namespace VNVTStore.Infrastructure;

public partial class TblSupplier
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? TaxCode { get; set; }

    public string? BankAccount { get; set; }

    public string? BankName { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<TblProduct> TblProducts { get; set; } = new List<TblProduct>();
}
