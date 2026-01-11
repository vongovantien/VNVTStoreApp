using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public partial class TblAddress
{
    public string Code { get; set; } = null!;

    public string UserCode { get; set; } = null!;

    public string AddressLine { get; set; } = null!;

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public bool? IsDefault { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TblOrder> TblOrders { get; set; } = new List<TblOrder>();

    public virtual TblUser UserCodeNavigation { get; set; } = null!;
}
