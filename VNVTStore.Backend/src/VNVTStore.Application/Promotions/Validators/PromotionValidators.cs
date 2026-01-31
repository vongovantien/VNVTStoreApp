using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Promotions.Validators;

public class CreatePromotionDtoValidator : AbstractValidator<CreatePromotionDto>
{
    public CreatePromotionDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Mã khuyến mãi không được để trống")
            .MaximumLength(50)
            .WithMessage("Mã khuyến mãi không được vượt quá 50 ký tự");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tên khuyến mãi không được để trống")
            .MaximumLength(200)
            .WithMessage("Tên khuyến mãi không được vượt quá 200 ký tự");

        RuleFor(x => x.DiscountType)
            .NotEmpty()
            .WithMessage("Loại giảm giá không được để trống")
            .Must(x => x == "PERCENTAGE" || x == "AMOUNT")
            .WithMessage("Loại giảm giá phải là 'PERCENTAGE' hoặc 'AMOUNT'");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("Giá trị giảm phải lớn hơn 0");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == "PERCENTAGE")
            .WithMessage("Phần trăm giảm không được vượt quá 100%");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("Ngày bắt đầu phải trước ngày kết thúc");

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinOrderAmount.HasValue)
            .WithMessage("Giá trị đơn hàng tối thiểu không được âm");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0)
            .When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("Giá trị giảm tối đa phải lớn hơn 0");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .When(x => x.UsageLimit.HasValue)
            .WithMessage("Số lần sử dụng phải lớn hơn 0");
    }
}

public class UpdatePromotionDtoValidator : AbstractValidator<UpdatePromotionDto>
{
    public UpdatePromotionDtoValidator()
    {
        Include(new CreatePromotionDtoValidator());
    }
}
