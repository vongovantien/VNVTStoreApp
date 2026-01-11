namespace VNVTStore.Application.Common;

public enum OrderStatus
{
    Pending,
    Processing,
    Paid,
    Shipped,
    Completed,
    Cancelled
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}
