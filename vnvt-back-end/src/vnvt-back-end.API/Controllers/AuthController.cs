using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using vnvt_back_end.Application.Interfaces;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.API.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _userService.AuthenticateAsync(request.Username, request.Password);
            if (token == null) return Unauthorized();

            return Ok(new { Token = token });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
