using FluentValidation;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Promotions.Validators;

public class CreatePromotionDtoValidator : AbstractValidator<CreatePromotionDto>
{
    public CreatePromotionDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DiscountType)
            .NotEmpty()
            .Must(x => x == "PERCENTAGE" || x == "AMOUNT")
            .WithMessage(_ => MessageConstants.Get(MessageConstants.DiscountTypeInvalid));

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0);

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == "PERCENTAGE");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate);

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinOrderAmount.HasValue);

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0)
            .When(x => x.MaxDiscountAmount.HasValue);

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .When(x => x.UsageLimit.HasValue);
    }
}

public class UpdatePromotionDtoValidator : AbstractValidator<UpdatePromotionDto>
{
    public UpdatePromotionDtoValidator()
    {
        Include(new CreatePromotionDtoValidator());
    }
}
