using FluentValidation;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Quotes.Validators;

public class CreateQuoteDtoValidator : AbstractValidator<CreateQuoteDto>
{
    public CreateQuoteDtoValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items).SetValidator(new CreateQuoteItemDtoValidator());

        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.CustomerName));

        RuleFor(x => x.CustomerEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.CustomerEmail));

        RuleFor(x => x.CustomerPhone)
            .Matches(@"^[0-9]{10,11}$")
            .When(x => !string.IsNullOrEmpty(x.CustomerPhone))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PhoneInvalid));

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Note));
    }
}

public class CreateQuoteItemDtoValidator : AbstractValidator<CreateQuoteItemDto>
{
    public CreateQuoteItemDtoValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}

public class UpdateQuoteDtoValidator : AbstractValidator<UpdateQuoteDto>
{
    public UpdateQuoteDtoValidator()
    {
        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TotalAmount.HasValue);

        RuleFor(x => x.AdminNote)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.AdminNote));
    }
}
