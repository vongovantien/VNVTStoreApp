using FluentValidation;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Quotes.Validators;

public class CreateQuoteDtoValidator : AbstractValidator<CreateQuoteDto>
{
    public CreateQuoteDtoValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Đơn báo giá phải có ít nhất một sản phẩm");

        RuleForEach(x => x.Items).SetValidator(new CreateQuoteItemDtoValidator());

        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.CustomerName))
            .WithMessage("Tên khách hàng không được vượt quá 100 ký tự");

        RuleFor(x => x.CustomerEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.CustomerEmail))
            .WithMessage("Email không hợp lệ");

        RuleFor(x => x.CustomerPhone)
            .Matches(@"^[0-9]{10,11}$")
            .When(x => !string.IsNullOrEmpty(x.CustomerPhone))
            .WithMessage("Số điện thoại phải có 10-11 chữ số");

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Note))
            .WithMessage("Ghi chú không được vượt quá 500 ký tự");
    }
}

public class CreateQuoteItemDtoValidator : AbstractValidator<CreateQuoteItemDto>
{
    public CreateQuoteItemDtoValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .WithMessage("Mã sản phẩm không được để trống");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Số lượng phải lớn hơn 0");
    }
}

public class UpdateQuoteDtoValidator : AbstractValidator<UpdateQuoteDto>
{
    public UpdateQuoteDtoValidator()
    {
        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TotalAmount.HasValue)
            .WithMessage("Tổng tiền không được âm");

        RuleFor(x => x.AdminNote)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.AdminNote))
            .WithMessage("Ghi chú admin không được vượt quá 500 ký tự");
    }
}
