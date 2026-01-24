using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Quotes.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Quotes.Handlers;

public class QuoteHandlers : BaseHandler<TblQuote>,
    IRequestHandler<CreateCommand<CreateQuoteDto, QuoteDto>, Result<QuoteDto>>,
    IRequestHandler<UpdateCommand<UpdateQuoteDto, QuoteDto>, Result<QuoteDto>>,
    IRequestHandler<DeleteCommand<TblQuote>, Result>,
    IRequestHandler<GetAllQuery<QuoteDto>, Result<IEnumerable<QuoteDto>>>,
    IRequestHandler<GetByCodeQuery<QuoteDto>, Result<QuoteDto>>,
    IRequestHandler<GetPagedQuery<QuoteDto>, Result<PagedResult<QuoteDto>>>,
    IRequestHandler<GetMyQuotesQuery, ApiResponse<List<QuoteDto>>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<TblProduct> _productRepository;

    public QuoteHandlers(
        IRepository<TblQuote> quoteRepository,
        IRepository<TblProduct> productRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(quoteRepository, unitOfWork, mapper)
    {
        _currentUser = currentUser;
        _productRepository = productRepository;
    }

    public async Task<Result<QuoteDto>> Handle(CreateCommand<CreateQuoteDto, QuoteDto> request, CancellationToken cancellationToken)
    {
        var userCode = _currentUser.UserCode;
        
        // Validate: Must have User or Guest Info
        if (string.IsNullOrEmpty(userCode))
        {
             if (string.IsNullOrEmpty(request.Dto.CustomerName) || string.IsNullOrEmpty(request.Dto.CustomerEmail))
             {
                  return Result.Failure<QuoteDto>(Error.Unauthorized(MessageConstants.RequireLoginOrGuestInfo));
             }
        }

        // Validate Product
        var product = await _productRepository.GetByCodeAsync(request.Dto.ProductCode, cancellationToken);
        if (product == null)
             return Result.Failure<QuoteDto>(Error.NotFound(MessageConstants.Product, request.Dto.ProductCode));

        return await CreateAsync<CreateQuoteDto, QuoteDto>(
            request.Dto,
            cancellationToken,
            q => {
                q.Code = Guid.NewGuid().ToString("N").Substring(0, 10);
                q.UserCode = userCode; // Can be null
                q.Status = "pending";
                q.CreatedAt = DateTime.Now;
                // AutoMapper should map CustomerName/Email/Phone/Company from DTO to Entity if names match.
                // CreateQuoteDto has CustomerName, CustomerEmail, CustomerPhone, Company.
                // TblQuote likely has them too.
            });
    }

    public async Task<Result<QuoteDto>> Handle(UpdateCommand<UpdateQuoteDto, QuoteDto> request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (quote == null)
            return Result.Failure<QuoteDto>(Error.NotFound("Quote", request.Code));

        var userCode = _currentUser.UserCode;
        var isAdmin = _currentUser.IsAdmin;

        if (quote.UserCode != userCode && !isAdmin)
            return Result.Failure<QuoteDto>(Error.Forbidden(MessageConstants.Forbidden));

        // If user, can only update if pending
        if (!isAdmin && quote.Status != "pending")
             return Result.Failure<QuoteDto>(Error.Conflict("Cannot update quote that is not pending"));

        return await UpdateAsync<UpdateQuoteDto, QuoteDto>(
            request.Code,
            request.Dto,
            "Quote",
            cancellationToken);
    }

    public async Task<Result> Handle(DeleteCommand<TblQuote> request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (quote == null)
            return Result.Failure(Error.NotFound("Quote", request.Code));

        var userCode = _currentUser.UserCode;
        var isAdmin = _currentUser.IsAdmin;

        if (quote.UserCode != userCode && !isAdmin)
            return Result.Failure(Error.Forbidden(MessageConstants.Forbidden));
            
        // If user, can only delete if pending? Or any status? Let's say pending only for now to be safe.
        // Actually typically users can delete/cancel request. 
        if (!isAdmin && quote.Status != "pending")
             return Result.Failure(Error.Conflict("Cannot delete quote that is not pending"));

        return await DeleteAsync(request.Code, "Quote", cancellationToken);
    }

    public async Task<Result<IEnumerable<QuoteDto>>> Handle(GetAllQuery<QuoteDto> request, CancellationToken cancellationToken)
    {
        // Get My Quotes
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode))
             return Result.Failure<IEnumerable<QuoteDto>>(Error.Unauthorized(MessageConstants.Unauthorized));

        var quotes = await _repository.AsQueryable()
            .Where(q => q.UserCode == userCode)
            .Include(q => q.ProductCodeNavigation)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<IEnumerable<QuoteDto>>(quotes));
    }

    public async Task<Result<QuoteDto>> Handle(GetByCodeQuery<QuoteDto> request, CancellationToken cancellationToken)
    {
        var quote = await _repository.AsQueryable()
            .Include(q => q.ProductCodeNavigation)
            .FirstOrDefaultAsync(q => q.Code == request.Code, cancellationToken);

        if (quote == null)
             return Result.Failure<QuoteDto>(Error.NotFound("Quote", request.Code));
             
        // Check access
        var userCode = _currentUser.UserCode;
        var isAdmin = _currentUser.IsAdmin;
        if (quote.UserCode != userCode && !isAdmin)
             return Result.Failure<QuoteDto>(Error.Forbidden(MessageConstants.Forbidden));

        return Result.Success(_mapper.Map<QuoteDto>(quote));
    }

    public async Task<ApiResponse<List<QuoteDto>>> Handle(GetMyQuotesQuery request, CancellationToken cancellationToken)
    {
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode))
        {
             return new ApiResponse<List<QuoteDto>> { Success = false, Message = "User not authenticated" };
        }

        var quotes = await _repository.AsQueryable()
            .Include(q => q.ProductCodeNavigation)
            .Where(q => q.UserCode == userCode)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuoteDto
            {
                Code = q.Code,
                UserCode = q.UserCode,
                ProductCode = q.ProductCode,
                ProductName = q.ProductCodeNavigation.Name,
                ProductImage = null,
                Quantity = q.Quantity,
                Note = q.Note,
                Status = q.Status,
                QuotedPrice = q.QuotedPrice,
                AdminNote = q.AdminNote,
                CreatedAt = q.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = q.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new ApiResponse<List<QuoteDto>> { Success = true, Data = quotes };
    }

    public async Task<Result<PagedResult<QuoteDto>>> Handle(GetPagedQuery<QuoteDto> request, CancellationToken cancellationToken)
    {
        return await GetPagedDapperAsync<QuoteDto>(
            request.PageIndex,
            request.PageSize,
            request.Searching,
            request.SortDTO,
            null,
            null,
            cancellationToken);
    }
}
