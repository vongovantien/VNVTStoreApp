using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.News.Validators;

/// <summary>
/// FluentValidation validators cho News DTOs
/// </summary>
public class CreateNewsDtoValidator : AbstractValidator<CreateNewsDto>
{
    public CreateNewsDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .MaximumLength(500).WithMessage("Tiêu đề không được vượt quá 500 ký tự");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung không được để trống");

        RuleFor(x => x.Slug)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Slug))
            .WithMessage("Slug không được vượt quá 500 ký tự")
            .Matches(@"^[a-z0-9-]+$").When(x => !string.IsNullOrEmpty(x.Slug))
            .WithMessage("Slug chỉ được chứa chữ thường, số và dấu gạch ngang");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.MetaTitle))
            .WithMessage("Meta title không được vượt quá 200 ký tự");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.MetaDescription))
            .WithMessage("Meta description không được vượt quá 500 ký tự");
    }
}

public class UpdateNewsDtoValidator : AbstractValidator<UpdateNewsDto>
{
    public UpdateNewsDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Tiêu đề không được vượt quá 500 ký tự");

        RuleFor(x => x.Slug)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Slug))
            .WithMessage("Slug không được vượt quá 500 ký tự")
            .Matches(@"^[a-z0-9-]+$").When(x => !string.IsNullOrEmpty(x.Slug))
            .WithMessage("Slug chỉ được chứa chữ thường, số và dấu gạch ngang");
    }
}
