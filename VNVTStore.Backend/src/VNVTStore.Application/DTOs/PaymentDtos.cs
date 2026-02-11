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
