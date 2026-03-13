using FluentValidation;
using VNVTStore.Application.Auth.Commands;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Auth.Validators;

public class RegisterCommandValidator : AbstractValidator<VNVTStore.Application.Auth.Commands.RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.username)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống")
            .MinimumLength(3).WithMessage("Tên đăng nhập phải có ít nhất 3 ký tự");

        RuleFor(x => x.email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không hợp lệ");

        RuleFor(x => x.password)
            .NotEmpty().WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .MinimumLength(8).WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak));
            
        RuleFor(x => x.fullName)
            .NotEmpty().WithMessage("Họ tên không được để trống");
    }
}

public class ResetPasswordCommandValidator : AbstractValidator<VNVTStore.Application.Auth.Commands.ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không hợp lệ");

        RuleFor(x => x.token)
            .NotEmpty().WithMessage("Token không được để trống");

        RuleFor(x => x.newPassword)
            .NotEmpty().WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .MinimumLength(8).WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak));
    }
}
