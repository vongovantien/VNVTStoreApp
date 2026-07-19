using FluentValidation;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.News.Validators;

/// <summary>
/// FluentValidation validators cho News DTOs
/// </summary>
public class CreateNewsDtoValidator : AbstractValidator<CreateNewsDto>
{
    public CreateNewsDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Content)
            .NotEmpty();

        RuleFor(x => x.Slug)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Slug))
            .Matches(@"^[a-z0-9-]+$").When(x => !string.IsNullOrEmpty(x.Slug))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.SlugInvalid));

        RuleFor(x => x.MetaTitle)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.MetaTitle));

        RuleFor(x => x.MetaDescription)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.MetaDescription));
    }
}

public class UpdateNewsDtoValidator : AbstractValidator<UpdateNewsDto>
{
    public UpdateNewsDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Slug)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Slug))
            .Matches(@"^[a-z0-9-]+$").When(x => !string.IsNullOrEmpty(x.Slug))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.SlugInvalid));
    }
}
