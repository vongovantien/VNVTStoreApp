using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Coupons.Validators;

public class CreateCouponDtoValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponDtoValidator()
    {
        RuleFor(x => x.PromotionCode)
            .NotEmpty()
            .WithMessage("Mã khuyến mãi không được để trống");
    }
}
