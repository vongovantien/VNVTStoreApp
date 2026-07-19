using System;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Domain.Entities;

public partial class TblDeliveryHistory
{
    public int Id { get; set; }
    public string DeliveryCode { get; set; } = null!;
    public DeliveryStatus Status { get; set; }
    public string? Note { get; set; }
    public string? Location { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UpdatedByCode { get; set; }

    public virtual TblDelivery DeliveryCodeNavigation { get; set; } = null!;
    public virtual TblUser? UpdatedByCodeNavigation { get; set; }
}
