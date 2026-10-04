using System;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public partial class TblDelivery : BaseEntity
{
    public string OrderCode { get; set; } = null!;
    public string? ShipperCode { get; set; }
    public string? ShipperName { get; set; }
    public string? ShipperPhone { get; set; }
    public string? TrackingNumber { get; set; }
    public string? CarrierName { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? Note { get; set; }
    public DeliveryStatus Status { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public virtual TblOrder OrderCodeNavigation { get; set; } = null!;
    public virtual TblUser? ShipperCodeNavigation { get; set; }
    
    public virtual ICollection<TblDeliveryHistory> TblDeliveryHistories { get; set; } = new List<TblDeliveryHistory>();
}
