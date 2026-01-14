using MediatR;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Events;

namespace VNVTStore.Application.Common.EventHandlers;

public class CartClearedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ICartService _cartService;

    public CartClearedEventHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(notification.CartCode))
        {
            await _cartService.ClearCartAsync(notification.CartCode, cancellationToken);
        }
    }
}
