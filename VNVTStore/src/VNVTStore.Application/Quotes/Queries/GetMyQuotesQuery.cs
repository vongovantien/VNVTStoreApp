using MediatR;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;


namespace VNVTStore.Application.Quotes.Queries;

public class GetMyQuotesQuery : IRequest<ApiResponse<List<QuoteDto>>>
{
    // Potentially add Pagination or Status filter here
}
