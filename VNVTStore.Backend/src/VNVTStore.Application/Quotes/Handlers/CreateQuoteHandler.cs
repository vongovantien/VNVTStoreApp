using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;


namespace VNVTStore.Application.Quotes.Handlers;

public class CreateQuoteHandler : IRequestHandler<Commands.CreateQuoteCommand, ApiResponse<QuoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    
    public CreateQuoteHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {

        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<QuoteDto>> Handle(Commands.CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        var userCode = _currentUser.UserCode;
        
        // Validation: Must have User or Guest Info
        if (string.IsNullOrEmpty(userCode))
        {
             if (string.IsNullOrEmpty(request.CustomerName) || string.IsNullOrEmpty(request.CustomerEmail))
             {
                  return new ApiResponse<QuoteDto> { Success = false, Message = MessageConstants.Get(MessageConstants.RequireLoginOrGuestInfo) };
             }
        }

        // Validate Product
        var product = await _context.TblProducts.FirstOrDefaultAsync(p => p.Code == request.ProductCode, cancellationToken);
        if (product == null)
        {
             return new ApiResponse<QuoteDto> { Success = false, Message = MessageConstants.Get(MessageConstants.EntityNotFound, MessageConstants.Product, request.ProductCode) };
        }

        // Let's create TblQuote
        var quote = new TblQuote
        {
            ProductCode = request.ProductCode,
            Quantity = request.Quantity,
            Note = request.Note,
            UserCode = userCode, // Can be null
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            Status = "pending",
            CreatedAt = DateTime.Now
        };

        _context.TblQuotes.Add(quote);
        await _context.SaveChangesAsync(cancellationToken);


        // Map to DTO
        var dto = new QuoteDto
        {
            Code = quote.Code,
            UserCode = quote.UserCode,
            ProductCode = quote.ProductCode,
            ProductName = product.Name,
            Quantity = quote.Quantity,
            Note = quote.Note,
            Status = quote.Status,
            CreatedAt = quote.CreatedAt ?? DateTime.UtcNow
        };

        return new ApiResponse<QuoteDto> { Success = true, Data = dto };
    }
}
