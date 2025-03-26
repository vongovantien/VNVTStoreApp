using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vnvt_back_end.Application.Interfaces;
using static vnvt_back_end.Application.DTOs.DTOs;
using vnvt_back_end.Application.Utils;

namespace vnvt_back_end.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICloudinaryService _cloudinaryService;

        public UsersController(IUserService userService, ICloudinaryService cloudinaryService)
        {
            _userService = userService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            if (avatar == null)
            {
                return BadRequest(ApiResponseBuilder.BadRequest<string>("Avatar file is required."));
            }

            var userId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponseBuilder.BadRequest<string>("User ID is missing."));
            }

            try
            {
                var url = await _cloudinaryService.UploadImageAsync(avatar);
                await _userService.UploadAvatar(int.Parse(userId), url);

                return Ok(ApiResponseBuilder.Success(new { AvatarUrl = url }, "Avatar uploaded successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseBuilder.BadRequest<string>($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst("id")?.Value);
            var profile = await _userService.GetUserProfileAsync(userId);

            if (profile == null)
            {
                return NotFound(ApiResponseBuilder.NotFound<UserDto>("User not found."));
            }

            return Ok(ApiResponseBuilder.Success(profile, "Profile retrieved successfully."));
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserDto profile)
        {
            if (profile == null)
            {
                return BadRequest(ApiResponseBuilder.BadRequest<string>("Invalid profile data."));
            }

            var userId = int.Parse(User.FindFirst("id")?.Value);
            await _userService.UpdateUserProfileAsync(userId, profile);

            return Ok(ApiResponseBuilder.Success<string>(null, "Profile updated successfully."));
        }
    }
}
