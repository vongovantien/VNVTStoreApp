using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Quotes.Handlers;

public class QuoteHandlers : BaseHandler<TblQuote>,
    IRequestHandler<CreateCommand<CreateQuoteDto, QuoteDto>, Result<QuoteDto>>,
    IRequestHandler<UpdateCommand<UpdateQuoteDto, QuoteDto>, Result<QuoteDto>>,
    IRequestHandler<DeleteCommand<TblQuote>, Result>,
    IRequestHandler<GetAllQuery<QuoteDto>, Result<IEnumerable<QuoteDto>>>,
    IRequestHandler<GetByCodeQuery<QuoteDto>, Result<QuoteDto>>
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
        if (string.IsNullOrEmpty(userCode))
             return Result.Failure<QuoteDto>(Error.Unauthorized(MessageConstants.Unauthorized));

        // Validate Product
        var product = await _productRepository.GetByCodeAsync(request.Dto.ProductCode, cancellationToken);
        if (product == null)
             return Result.Failure<QuoteDto>(Error.NotFound(MessageConstants.Product, request.Dto.ProductCode));

        return await CreateAsync<CreateQuoteDto, QuoteDto>(
            request.Dto,
            cancellationToken,
            q => {
                q.Code = Guid.NewGuid().ToString("N").Substring(0, 10);
                q.UserCode = userCode;
                q.Status = "pending";
                q.CreatedAt = DateTime.UtcNow;
            });
    }

    public async Task<Result<QuoteDto>> Handle(UpdateCommand<UpdateQuoteDto, QuoteDto> request, CancellationToken cancellationToken)
    {
        var quote = await Repository.GetByCodeAsync(request.Code, cancellationToken);
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
        var quote = await Repository.GetByCodeAsync(request.Code, cancellationToken);
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

        var quotes = await Repository.AsQueryable()
            .Where(q => q.UserCode == userCode)
            .Include(q => q.ProductCodeNavigation)
            .ThenInclude(p => p.TblProductImages)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(Mapper.Map<IEnumerable<QuoteDto>>(quotes));
    }

    public async Task<Result<QuoteDto>> Handle(GetByCodeQuery<QuoteDto> request, CancellationToken cancellationToken)
    {
        var quote = await Repository.AsQueryable()
            .Include(q => q.ProductCodeNavigation)
            .ThenInclude(p => p.TblProductImages)
            .FirstOrDefaultAsync(q => q.Code == request.Code, cancellationToken);

        if (quote == null)
             return Result.Failure<QuoteDto>(Error.NotFound("Quote", request.Code));
             
        // Check access
        var userCode = _currentUser.UserCode;
        var isAdmin = _currentUser.IsAdmin;
        if (quote.UserCode != userCode && !isAdmin)
             return Result.Failure<QuoteDto>(Error.Forbidden(MessageConstants.Forbidden));

        return Result.Success(Mapper.Map<QuoteDto>(quote));
    }
}
