using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Products.Validators;

/// <summary>
/// FluentValidation validators cho Product DTOs
/// </summary>
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
            .MaximumLength(200).WithMessage("Tên sản phẩm không được vượt quá 200 ký tự");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Giá sản phẩm phải lớn hơn 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).When(x => x.StockQuantity.HasValue)
            .WithMessage("Số lượng tồn kho không được âm");

        RuleFor(x => x.VatRate)
            .InclusiveBetween(0, 100).When(x => x.VatRate.HasValue)
            .WithMessage("Thuế VAT phải từ 0% đến 100%");

        RuleFor(x => x.MinStockLevel)
            .GreaterThanOrEqualTo(0).When(x => x.MinStockLevel.HasValue)
            .WithMessage("Mức tồn kho tối thiểu không được âm");
    }
}

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Tên sản phẩm không được vượt quá 200 ký tự");

        RuleFor(x => x.Price)
            .GreaterThan(0).When(x => x.Price.HasValue)
            .WithMessage("Giá sản phẩm phải lớn hơn 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).When(x => x.StockQuantity.HasValue)
            .WithMessage("Số lượng tồn kho không được âm");
    }
}

public class CreateProductVariantDtoValidator : AbstractValidator<CreateProductVariantDto>
{
    public CreateProductVariantDtoValidator()
    {
        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU không được để trống")
            .MaximumLength(50).WithMessage("SKU không được vượt quá 50 ký tự");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Giá biến thể phải lớn hơn 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Số lượng tồn kho không được âm");
    }
}
