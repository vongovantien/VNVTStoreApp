using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Addresses.Validators;

public class CreateAddressDtoValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressDtoValidator()
    {
        RuleFor(x => x.AddressLine)
            .NotEmpty()
            .WithMessage("Địa chỉ không được để trống")
            .MaximumLength(500)
            .WithMessage("Địa chỉ không được vượt quá 500 ký tự");

        RuleFor(x => x.FullName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.FullName))
            .WithMessage("Họ tên không được vượt quá 100 ký tự");

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Số điện thoại phải có 10-11 chữ số");

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.City))
            .WithMessage("Thành phố không được vượt quá 100 ký tự");

        RuleFor(x => x.PostalCode)
            .MaximumLength(10)
            .When(x => !string.IsNullOrEmpty(x.PostalCode))
            .WithMessage("Mã bưu điện không được vượt quá 10 ký tự");
    }
}

public class UpdateAddressDtoValidator : AbstractValidator<UpdateAddressDto>
{
    public UpdateAddressDtoValidator()
    {
        RuleFor(x => x.AddressLine)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.AddressLine))
            .WithMessage("Địa chỉ không được vượt quá 500 ký tự");

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10,11}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Số điện thoại phải có 10-11 chữ số");
    }
}
