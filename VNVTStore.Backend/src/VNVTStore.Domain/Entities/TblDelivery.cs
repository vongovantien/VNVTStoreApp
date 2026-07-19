using System;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblDelivery : IEntity
{
    public string Code { get; set; } = null!;
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
    
    public string? ModifiedType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual TblOrder OrderCodeNavigation { get; set; } = null!;
    public virtual TblUser? ShipperCodeNavigation { get; set; }
    
    public virtual ICollection<TblDeliveryHistory> TblDeliveryHistories { get; set; } = new List<TblDeliveryHistory>();
}
