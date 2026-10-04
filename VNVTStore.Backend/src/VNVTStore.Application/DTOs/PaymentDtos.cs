namespace VNVTStore.Application.DTOs;

public class PaymentDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string OrderCode { get; set; } = null!;
    public DateTime? PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = null!;
    public string? TransactionId { get; set; }
    public string? Status { get; set; }
}

public class PaymentUrlDto
{
    public string OrderCode { get; set; } = null!;
    public string PaymentCode { get; set; } = null!;
    public string PaymentUrl { get; set; } = null!;
}

public enum PaymentConfirmOutcome
{
    /// <summary>Payment succeeded and was applied now.</summary>
    Confirmed,
    /// <summary>Payment had already been confirmed earlier (duplicate callback).</summary>
    AlreadyConfirmed,
    /// <summary>Gateway reported failure / customer cancelled.</summary>
    Failed,
    InvalidSignature,
    NotFound,
    InvalidAmount
}

public class PaymentConfirmationDto
{
    public PaymentConfirmOutcome Outcome { get; set; }
    public bool IsSuccess => Outcome is PaymentConfirmOutcome.Confirmed or PaymentConfirmOutcome.AlreadyConfirmed;
    public string? OrderCode { get; set; }
    public string? PaymentCode { get; set; }
    public decimal Amount { get; set; }
    public string? ResponseCode { get; set; }
    public string? Message { get; set; }
}
