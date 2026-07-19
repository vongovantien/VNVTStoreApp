using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Coupons.Validators;

public class CreateCouponDtoValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponDtoValidator()
    {
        RuleFor(x => x.PromotionCode)
            .NotEmpty();
    }
}
