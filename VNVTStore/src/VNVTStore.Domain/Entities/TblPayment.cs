using System;
using System.Collections.Generic;

namespace VNVTStore.Infrastructure;

public partial class TblPayment
{
    public string Code { get; set; } = null!;

    public string OrderCode { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string Method { get; set; } = null!;

    public string? TransactionId { get; set; }

    public string? Status { get; set; }

    public virtual TblOrder OrderCodeNavigation { get; set; } = null!;
}
