using System;
using System.Collections.Generic;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.DTOs;

public class DeliveryDto
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
    public string StatusText => Status.ToString();
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<DeliveryHistoryDto> Histories { get; set; } = new List<DeliveryHistoryDto>();
}

public class DeliveryHistoryDto
{
    public int Id { get; set; }
    public string DeliveryCode { get; set; } = null!;
    public DeliveryStatus Status { get; set; }
    public string StatusText => Status.ToString();
    public string? Note { get; set; }
    public string? Location { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UpdatedByCode { get; set; }
    public string? UpdatedByName { get; set; }
}

public class AssignDeliveryDto
{
    public string? ShipperCode { get; set; }
    public string? ShipperName { get; set; }
    public string? ShipperPhone { get; set; }
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? Note { get; set; }
}

public class UpdateDeliveryStatusDto
{
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public string? Location { get; set; }
}
