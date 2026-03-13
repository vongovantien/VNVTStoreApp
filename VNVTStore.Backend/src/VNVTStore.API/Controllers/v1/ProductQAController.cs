using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Products.Commands;
using VNVTStore.Application.Products.Queries;
using VNVTStore.Domain.Enums;

using MediatR;
namespace VNVTStore.API.Controllers.v1;

/// <summary>
/// Managing Product Questions & Answers
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Product QA")]
public class ProductQAController : BaseApiController
{
    public ProductQAController(IMediator mediator) : base(mediator)
    {
    }
    /// <summary>
    /// Get questions for a specific product
    /// </summary>
    /// <param name="productCode">Product code</param>
    /// <returns>List of questions with answers</returns>
    [HttpGet("{productCode}")]
    [ProducesResponseType(typeof(ApiResponse<List<ReviewDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestions(string productCode)
    {
        var result = await Mediator.Send(new GetProductQuestionsQuery(productCode));
        return HandleResult(result);
    }

    /// <summary>
    /// Submit a question for a product (Requires Login)
    /// </summary>
    /// <param name="command">Question details</param>
    /// <returns>Success status</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
