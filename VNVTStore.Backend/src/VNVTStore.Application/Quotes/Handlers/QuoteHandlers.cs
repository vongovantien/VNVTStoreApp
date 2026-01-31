using AutoMapper;
using Serilog;
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
    IRequestHandler<GetMyQuotesQuery, Result<List<QuoteDto>>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IRepository<TblUser> _userRepository;
    private readonly IEmailService _emailService;

    public QuoteHandlers(
        IRepository<TblQuote> quoteRepository,
        IRepository<TblProduct> productRepository,
        IRepository<TblUser> userRepository,
        IEmailService emailService,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(quoteRepository, unitOfWork, mapper)
    {
        _currentUser = currentUser;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _emailService = emailService;
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

        // Create Quote Entity
        var quote = new TblQuote
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            UserCode = userCode,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            CustomerName = request.Dto.CustomerName,
            CustomerEmail = request.Dto.CustomerEmail,
            CustomerPhone = request.Dto.CustomerPhone,
            Note = request.Dto.Note
        };

        // Add Items
        if (request.Dto.Items != null && request.Dto.Items.Any())
        {
            foreach (var itemDto in request.Dto.Items)
            {
                // Validate Product
                var product = await _productRepository.GetByCodeAsync(itemDto.ProductCode, cancellationToken);
                if (product == null) continue; 

                var quoteItem = new TblQuoteItem
                {
                    Code = Guid.NewGuid().ToString("N"),
                    QuoteCode = quote.Code,
                    ProductCode = itemDto.ProductCode,
                    Quantity = itemDto.Quantity,
                    UnitCode = itemDto.UnitCode,
                    RequestPrice = itemDto.RequestPrice ?? 0,
                    ApprovedPrice = 0 
                };
                quote.TblQuoteItems.Add(quoteItem);
            }
        }
        else
        {
            return Result.Failure<QuoteDto>(Error.Validation("Items", "Quote must have at least one item"));
        }

        await _repository.AddAsync(quote, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        // Notify Admins
        try
        {
            var admins = await _userRepository.AsQueryable()
                .Where(u => u.Role == VNVTStore.Domain.Enums.UserRole.Admin && u.IsActive && !string.IsNullOrEmpty(u.Email))
                .ToListAsync(cancellationToken);

            if (admins.Any())
            {
                var subject = $"[New Quote Request] {quote.Code} - {quote.CustomerName}";
                var body = $@"
                    <h3>New Quote Submission</h3>
                    <p><strong>Quote Code:</strong> {quote.Code}</p>
                    <p><strong>Customer:</strong> {quote.CustomerName} ({quote.CustomerEmail})</p>
                    <p><strong>Date:</strong> {quote.CreatedAt:yyyy-MM-dd HH:mm:ss}</p>
                    <p><strong>Note:</strong> {quote.Note ?? "N/A"}</p>
                    <hr/>
                    <p>Please log in to the admin panel to review and approve this quote.</p>";

                foreach (var admin in admins)
                {
                    await _emailService.SendEmailAsync(admin.Email, subject, body, true);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request
            Serilog.Log.Error(ex, "Failed to send admin notification for quote {QuoteCode}", quote.Code);
        }

        return Result.Success(_mapper.Map<QuoteDto>(quote));
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
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode))
             return Result.Failure<IEnumerable<QuoteDto>>(Error.Unauthorized(MessageConstants.Unauthorized));

        IQueryable<TblQuote> query = _repository.AsQueryable()
            .Include(q => q.TblQuoteItems)
                .ThenInclude(i => i.Product);

        if (!_currentUser.IsAdmin)
        {
            query = query.Where(q => q.UserCode == userCode);
        }

        var quotes = await query.OrderByDescending(q => q.CreatedAt).ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<IEnumerable<QuoteDto>>(quotes));
    }

    public async Task<Result<QuoteDto>> Handle(GetByCodeQuery<QuoteDto> request, CancellationToken cancellationToken)
    {
        var quote = await _repository.AsQueryable()
            .Include(q => q.TblQuoteItems)
                .ThenInclude(i => i.Product)
            .Include(q => q.TblQuoteItems)
                .ThenInclude(i => i.Unit)
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

    public async Task<Result<List<QuoteDto>>> Handle(GetMyQuotesQuery request, CancellationToken cancellationToken)
    {
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode))
        {
             return Result.Failure<List<QuoteDto>>(Error.Unauthorized("User not authenticated"));
        }

        var quotes = await _repository.AsQueryable()
            .Include(q => q.TblQuoteItems)
                .ThenInclude(i => i.Product)
            .Where(q => q.UserCode == userCode)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<List<QuoteDto>>(quotes));
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
