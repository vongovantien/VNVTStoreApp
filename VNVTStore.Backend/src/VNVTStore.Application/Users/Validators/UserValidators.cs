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
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống")
            .MinimumLength(3).WithMessage("Tên đăng nhập phải có ít nhất 3 ký tự")
            .MaximumLength(50).WithMessage("Tên đăng nhập không được vượt quá 50 ký tự")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Tên đăng nhập chỉ được chứa chữ, số và dấu gạch dưới");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không hợp lệ")
            .MaximumLength(100).WithMessage("Email không được vượt quá 100 ký tự");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .MinimumLength(8).WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak));

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Số điện thoại phải có 10-11 chữ số");
    }
}

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email không hợp lệ");

        RuleFor(x => x.Password)
            .MinimumLength(8).When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak))
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage(MessageConstants.Get(MessageConstants.PasswordTooWeak));

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Số điện thoại phải có 10-11 chữ số");
    }
}
