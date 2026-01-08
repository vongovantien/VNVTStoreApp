using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Categories.Queries;
using VNVTStore.Application.Categories.Commands;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.API.Controllers.v1;

[Route("api/v1/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("search")]
    public async Task<ActionResult<Result<PagedResult<CategoryDto>>>> Search([FromBody] RequestDTO request)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(request));
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
    
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<CategoryDto>>>> GetAll()
    {
        var request = new RequestDTO { PageIndex = 1, PageSize = 1000 };
        var result = await _mediator.Send(new GetCategoriesQuery(request));
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<CategoryDto>>> Create([FromBody] CategoryDto dto)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(dto));
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
