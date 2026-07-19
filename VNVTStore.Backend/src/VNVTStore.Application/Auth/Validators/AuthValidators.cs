using FluentValidation;
using VNVTStore.Application.Auth.Commands;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Auth.Validators;

public class RegisterCommandValidator : AbstractValidator<VNVTStore.Application.Auth.Commands.RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.username)
            .NotEmpty()
            .MinimumLength(3);

        RuleFor(x => x.email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.password)
            .NotEmpty().WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .MinimumLength(8).WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak));
            
        RuleFor(x => x.fullName)
            .NotEmpty();
    }
}

public class ResetPasswordCommandValidator : AbstractValidator<VNVTStore.Application.Auth.Commands.ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.token)
            .NotEmpty();

        RuleFor(x => x.newPassword)
            .NotEmpty().WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .MinimumLength(8).WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak));
    }
}
