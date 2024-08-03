using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.API.Controllers
{
    namespace WebAPI.Controllers
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
                await _userService.RegisterAsync(request.Username, request.Email, request.Password);
                var response = new ApiResponse<string>(
                    success: true,
                    message: "User registered successfully.",
                    data: null,
                    statusCode: 200
                );
                return Ok(response);
            }

            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginRequest request)
            {
                var token = await _userService.AuthenticateAsync(request.Username, request.Password);
                if (token == null)
                {
                    var errorResponse = new ApiResponse<string>(
                        success: false,
                        message: "Invalid username or password.",
                        data: null,
                        statusCode: 401
                    );
                    return Unauthorized(errorResponse);
                }

                var response = new ApiResponse<string>(
                    success: true,
                    message: "Login successful.",
                    data: token,
                    statusCode: 200
                );
                return Ok(response);
            }

            [HttpPost("forgot-password")]
            public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
            {
                await _userService.ForgotPasswordAsync(request.Email);
                var response = new ApiResponse<string>(
                    success: true,
                    message: "Password reset email sent.",
                    data: null,
                    statusCode: 200
                );
                return Ok(response);
            }

            [HttpPost("reset-password")]
            public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
            {
                await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
                var response = new ApiResponse<string>(
                    success: true,
                    message: "Password has been reset.",
                    data: null,
                    statusCode: 200
                );
                return Ok(response);
            }

            [Authorize]
            [HttpGet("profile")]
            public async Task<IActionResult> GetProfile()
            {
                var userId = int.Parse(User.FindFirst("id").Value);
                var profile = await _userService.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    var errorResponse = new ApiResponse<string>(
                        success: false,
                        message: "User not found.",
                        data: null,
                        statusCode: 404
                    );
                    return NotFound(errorResponse);
                }

                var response = new ApiResponse<UserProfileDto>(
                    success: true,
                    message: "Profile retrieved successfully.",
                    data: profile,
                    statusCode: 200
                );
                return Ok(response);
            }

            [Authorize]
            [HttpPut("profile")]
            public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto profile)
            {
                var userId = int.Parse(User.FindFirst("id").Value);
                await _userService.UpdateUserProfileAsync(userId, profile);

                var response = new ApiResponse<string>(
                    success: true,
                    message: "Profile updated successfully.",
                    data: null,
                    statusCode: 200
                );
                return Ok(response);
            }
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
}
