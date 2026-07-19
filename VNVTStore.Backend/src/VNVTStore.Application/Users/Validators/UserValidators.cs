using FluentValidation;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Users.Validators;

/// <summary>
/// FluentValidation validators cho User DTOs
/// </summary>
public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage(_ => MessageConstants.Get(MessageConstants.UsernameInvalid));

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .MinimumLength(8).WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak));

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PhoneInvalid));
    }
}

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Password)
            .MinimumLength(8).When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak));

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PhoneInvalid));
    }
}
