using FluentValidation;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Addresses.Validators;

public class CreateAddressDtoValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressDtoValidator()
    {
        RuleFor(x => x.AddressLine)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.FullName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PhoneInvalid));

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.PostalCode)
            .MaximumLength(10)
            .When(x => !string.IsNullOrEmpty(x.PostalCode));
    }
}

public class UpdateAddressDtoValidator : AbstractValidator<UpdateAddressDto>
{
    public UpdateAddressDtoValidator()
    {
        RuleFor(x => x.AddressLine)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.AddressLine));

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PhoneInvalid));
    }
}
