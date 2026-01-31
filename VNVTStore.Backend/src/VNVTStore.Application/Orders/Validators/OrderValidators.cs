using FluentValidation;
using VNVTStore.Application.Orders.Commands;

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
            .WithMessage("Đơn hàng phải có giỏ hàng hoặc danh sách sản phẩm");

        When(x => x.Items != null && x.Items.Count > 0, () =>
        {
            RuleForEach(x => x.Items).SetValidator(new OrderCreationItemDtoValidator());
        });

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Số điện thoại phải có 10-11 chữ số");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email không hợp lệ");
    }
}

public class OrderCreationItemDtoValidator : AbstractValidator<OrderCreationItemDto>
{
    public OrderCreationItemDtoValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty().WithMessage("Mã sản phẩm không được để trống");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
    }
}
