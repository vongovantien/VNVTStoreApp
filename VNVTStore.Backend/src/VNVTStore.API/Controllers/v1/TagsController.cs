using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

public class TagsController : BaseApiController<TblTag, TagDto, CreateTagDto, UpdateTagDto>
{
    public TagsController(IMediator mediator) : base(mediator)
    {
    }
}
