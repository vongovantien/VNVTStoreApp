using MediatR;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Domain.Events;

public class OrderCreatedEvent : INotification
{
    public TblOrder Order { get; }
    public string UserCode { get; }
    public string? CartCode { get; }

    public OrderCreatedEvent(TblOrder order, string userCode, string? cartCode)
    {
        Order = order;
        UserCode = userCode;
        CartCode = cartCode;
    }
}
