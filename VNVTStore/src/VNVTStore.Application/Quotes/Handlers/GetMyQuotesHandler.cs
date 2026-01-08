using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Quotes.Queries;

namespace VNVTStore.Application.Quotes.Handlers;

public class GetMyQuotesHandler : IRequestHandler<GetMyQuotesQuery, ApiResponse<List<QuoteDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetMyQuotesHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {

        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<QuoteDto>>> Handle(GetMyQuotesQuery request, CancellationToken cancellationToken)
    {
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode))
        {
             return new ApiResponse<List<QuoteDto>> { Success = false, Message = "User not authenticated" };
        }

        var quotes = await _context.TblQuotes
            .Include(q => q.ProductCodeNavigation)
            .Where(q => q.UserCode == userCode)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuoteDto
            {
                Code = q.Code,
                UserCode = q.UserCode,
                ProductCode = q.ProductCode,
                ProductName = q.ProductCodeNavigation.Name,
                ProductImage = q.ProductCodeNavigation.TblProductImages
                                .Where(img => img.IsPrimary == true)
                                .Select(img => img.ImageUrl)
                                .FirstOrDefault(), // Fetch Primary Image
                Quantity = q.Quantity,
                Note = q.Note,
                Status = q.Status,
                QuotedPrice = q.QuotedPrice,
                AdminNote = q.AdminNote,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new ApiResponse<List<QuoteDto>> { Success = true, Data = quotes };
    }
}
