using FluentValidation;
using VNVTStore.Application.DTOs;
using System;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Banners.Validators;

public class CreateBannerDtoValidator : AbstractValidator<CreateBannerDto>
{
    public CreateBannerDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.LinkUrl)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.LinkUrl))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.UrlInvalid));

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Content)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Content));
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
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Priority.HasValue);
    }
}
