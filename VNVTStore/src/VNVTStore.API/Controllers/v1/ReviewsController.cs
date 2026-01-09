using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Reviews.Commands;
using VNVTStore.Application.Reviews.Queries;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ReviewsController : BaseApiController<ReviewDto, CreateReviewDto, UpdateReviewDto>
{
    private readonly ICurrentUser _currentUser;

    public ReviewsController(IMediator mediator, ICurrentUser currentUser) : base(mediator)
    {
        _currentUser = currentUser;
    }

    private string GetUserCode() => _currentUser.UserCode ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Get reviews for a product
    /// </summary>
    [HttpGet("product/{productCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductReviews(string productCode, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetProductReviewsQuery(productCode, pageIndex, pageSize));
        return HandleResult(result);
    }

    /// <summary>
    /// Get current user's reviews
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews()
    {
        var result = await Mediator.Send(new GetUserReviewsQuery(GetUserCode()));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost]
    public override async Task<IActionResult> Create([FromBody] RequestDTO<CreateReviewDto> request)
    {
        request.PostObject!.UserCode = GetUserCode();
        return await base.Create(request);
    }

    [Authorize]
    [HttpPut("{code}")]
    public override async Task<IActionResult> Update(string code, [FromBody] RequestDTO<UpdateReviewDto> request)
    {
        // For Update, we pass UserCode to the command factory
        return await base.Update(code, request);
    }

    [Authorize]
    [HttpDelete("{code}")]
    public override async Task<IActionResult> Delete(string code)
    {
        return await base.Delete(code);
    }

    /// <summary>
    /// Get all reviews (Admin moderation)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllReviews([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isApproved = null)
    {
        var result = await Mediator.Send(new GetAllReviewsQuery(pageIndex, pageSize, isApproved));
        return HandleResult(result);
    }

    /// <summary>
    /// Approve a review (Admin)
    /// </summary>
    [HttpPost("{code}/approve")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ApproveReview(string code)
    {
        var result = await Mediator.Send(new ApproveReviewCommand(code));
        return HandleDelete(result);
    }

    /// <summary>
    /// Reject a review (Admin)
    /// </summary>
    [HttpPost("{code}/reject")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RejectReview(string code)
    {
        var result = await Mediator.Send(new RejectReviewCommand(code));
        return HandleDelete(result);
    }

    protected override IRequest<Result<PagedResult<ReviewDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort)
        => new GetPagedQuery<ReviewDto>(pageIndex, pageSize, search, sort);

    protected override IRequest<Result<ReviewDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<ReviewDto>(code);

    protected override IRequest<Result<ReviewDto>> CreateCreateCommand(CreateReviewDto dto)
        => new CreateCommand<CreateReviewDto, ReviewDto>(dto);

    protected override IRequest<Result<ReviewDto>> CreateUpdateCommand(string code, UpdateReviewDto dto)
        => new UpdateCommand<UpdateReviewDto, ReviewDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblReview>(code);
}
