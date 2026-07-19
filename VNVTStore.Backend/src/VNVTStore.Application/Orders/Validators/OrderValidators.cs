using FluentValidation;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Orders.Validators;

/// <summary>
/// FluentValidation validators cho Order DTOs
/// </summary>
public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        // Must have either CartCode (for logged users) or Items (for guests)
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.CartCode) || (x.Items != null && x.Items.Count > 0))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.OrderOrCartRequired));

        When(x => x.Items != null && x.Items.Count > 0, () =>
        {
            RuleForEach(x => x.Items).SetValidator(new OrderCreationItemDtoValidator());
        });

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PhoneInvalid));

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class OrderCreationItemDtoValidator : AbstractValidator<OrderCreationItemDto>
{
    public OrderCreationItemDtoValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
