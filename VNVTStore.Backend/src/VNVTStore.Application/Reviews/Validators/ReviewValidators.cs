using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Reviews.Validators;

public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewDtoValidator()
    {
        RuleFor(x => x.UserCode)
            .NotEmpty()
            .WithMessage("Mã người dùng không được để trống");

        RuleFor(x => x.OrderItemCode)
            .NotEmpty()
            .WithMessage("Mã sản phẩm đã mua không được để trống");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .When(x => x.Rating.HasValue)
            .WithMessage("Đánh giá phải từ 1 đến 5 sao");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage("Nhận xét không được vượt quá 1000 ký tự");
    }
}

public class UpdateReviewDtoValidator : AbstractValidator<UpdateReviewDto>
{
    public UpdateReviewDtoValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .When(x => x.Rating.HasValue)
            .WithMessage("Đánh giá phải từ 1 đến 5 sao");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage("Nhận xét không được vượt quá 1000 ký tự");

        RuleFor(x => x.AdminReply)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.AdminReply))
            .WithMessage("Phản hồi admin không được vượt quá 500 ký tự");
    }
}
