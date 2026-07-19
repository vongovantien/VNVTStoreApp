using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Brands.Validators;

/// <summary>
/// FluentValidation validators cho Brand DTOs
/// </summary>
public class CreateBrandDtoValidator : AbstractValidator<CreateBrandDto>
{
    public CreateBrandDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class UpdateBrandDtoValidator : AbstractValidator<UpdateBrandDto>
{
    public UpdateBrandDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name));
    }
}
