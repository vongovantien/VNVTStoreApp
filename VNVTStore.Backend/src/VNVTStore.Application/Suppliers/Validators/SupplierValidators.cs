using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Suppliers.Validators;

public class CreateSupplierDtoValidator : AbstractValidator<CreateSupplierDto>
{
    public CreateSupplierDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tên nhà cung cấp không được để trống")
            .MaximumLength(200)
            .WithMessage("Tên nhà cung cấp không được vượt quá 200 ký tự");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email không hợp lệ");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Số điện thoại không được vượt quá 20 ký tự");

        RuleFor(x => x.TaxCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.TaxCode))
            .WithMessage("Mã số thuế không được vượt quá 20 ký tự");
    }
}

public class UpdateSupplierDtoValidator : AbstractValidator<UpdateSupplierDto>
{
    public UpdateSupplierDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Tên nhà cung cấp không được vượt quá 200 ký tự");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email không hợp lệ");
    }
}
