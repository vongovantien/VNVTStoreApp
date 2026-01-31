using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Banners.Validators;

public class CreateBannerDtoValidator : AbstractValidator<CreateBannerDto>
{
    public CreateBannerDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Tiêu đề banner không được để trống")
            .MaximumLength(200)
            .WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        RuleFor(x => x.LinkUrl)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.LinkUrl))
            .WithMessage("URL liên kết không hợp lệ");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Độ ưu tiên phải >= 0");

        RuleFor(x => x.Content)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Content))
            .WithMessage("Nội dung không được vượt quá 500 ký tự");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _);
    }
}

public class UpdateBannerDtoValidator : AbstractValidator<UpdateBannerDto>
{
    public UpdateBannerDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Priority.HasValue)
            .WithMessage("Độ ưu tiên phải >= 0");
    }
}
