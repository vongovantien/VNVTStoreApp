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

        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> GetSystemConfigs()
        {
            var result = await Mediator.Send(new GetSystemConfigsQuery());
            return HandleResult(result);
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

        [HttpGet("export")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Export()
        {
            var result = await Mediator.Send(new ExportSystemConfigsQuery());
            if (!result.IsSuccess) return HandleResult(result);
            return File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SystemConfigs_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpPost("import")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is empty");
            using var stream = file.OpenReadStream();
            var result = await Mediator.Send(new ImportSystemConfigsCommand(stream));
            return HandleResult(result);
        }
    }
}
