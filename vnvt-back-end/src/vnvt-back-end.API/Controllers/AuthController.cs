using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Utils;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _userService.CheckUserExisted(request.Username))
            {
                return BadRequest(ApiResponseBuilder.BadRequest<string>("User already exists."));
            }

            await _userService.RegisterAsync(request.Username, request.Email, request.Password);
            return Ok(ApiResponseBuilder.Success<string>(null, "User registered successfully."));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(ApiResponseBuilder.BadRequest<string>("Username and password are required."));
            }

            var user = await _userService.AuthenticateAsync(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(ApiResponseBuilder.Unauthorized<string>("Invalid username or password."));
            }

            return Ok(ApiResponseBuilder.Success(user, "Login successful."));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _userService.ForgotPasswordAsync(request.Email);
            return Ok(ApiResponseBuilder.Success<string>(null, "Password reset email sent."));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
            return Ok(ApiResponseBuilder.Success<string>(null, "Password has been reset."));
        }

        //[HttpPost("refresh-token")]
        //public IActionResult RefreshToken()
        //{
        //    // Validate the refresh token and generate a new JWT token
        //    var token = _userService.GetRefreshTokenAsync();
        //    return Ok(ApiResponseBuilder.Success<string>(token, null));
        //}
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
