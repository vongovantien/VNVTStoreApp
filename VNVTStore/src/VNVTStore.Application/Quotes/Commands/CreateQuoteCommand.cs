using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;


namespace VNVTStore.Application.Quotes.Commands;

public class CreateQuoteCommand : IRequest<ApiResponse<QuoteDto>>
{
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Note { get; set; }
}
