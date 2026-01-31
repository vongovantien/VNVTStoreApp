using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

[Route("api/v1/[controller]")]
[ApiController]
public class NewsController : BaseApiController<TblNews, NewsDto, CreateNewsDto, UpdateNewsDto>
{
    public NewsController(IMediator mediator) : base(mediator)
    {
    }
}
