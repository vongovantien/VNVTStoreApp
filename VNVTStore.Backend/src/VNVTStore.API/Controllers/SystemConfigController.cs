using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.SystemConfig.Commands;
using MediatR;
using VNVTStore.Application.SystemConfig.Queries;

namespace VNVTStore.API.Controllers
{
    [Authorize]
    public class SystemConfigController : BaseApiController
    {
        public SystemConfigController(IMediator mediator) : base(mediator)
        {
        }
        [HttpGet("{key}")]
        [AllowAnonymous] // Allow public access for Flash Sales etc.
        public async Task<IActionResult> Get(string key)
        {
            var result = await Mediator.Send(new GetSystemConfigQuery { ConfigKey = key });
            return HandleResult(result);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin))] // Only admin can update
        public async Task<IActionResult> Update([FromBody] UpdateSystemConfigCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}
