using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<string> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAndPasswordAsync(username, password);
            if (user == null) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationInMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task RegisterAsync(string username, string email, string password)
        {
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            await _userRepository.CreateUserAsync(user);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            //var user = await _userRepository.GetUserByEmailAsync(email);
            //if (user == null) return;

            //var token = Guid.NewGuid().ToString();
            //var expiration = DateTime.UtcNow.AddHours(1);
            //await _passwordResetTokenRepository.CreatePasswordResetTokenAsync(new PasswordResetToken
            //{
            //    Email = email,
            //    Token = token,
            //    Expiration = expiration
            //});
            //var resetLink = $"https://yourapp.com/reset-password?token={token}";
            //await _emailService.SendEmailAsync(email, "Reset Your Password", $"Click <a href='{resetLink}'>here</a> to reset your password.");
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            //var resetToken = await _passwordResetTokenRepository.GetPasswordResetTokenAsync(token);
            //if (resetToken == null || resetToken.Expiration < DateTime.UtcNow) return;

            //var user = await _userRepository.GetUserByEmailAsync(resetToken.Email);
            //if (user == null) return;

            //user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            //await _userRepository.UpdateUserAsync(user);

            //await _passwordResetTokenRepository.DeletePasswordResetTokenAsync(resetToken.Id);
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                Username = user.Username,
                Email = user.Email,
                // Map other properties
            };
        }

        public async Task UpdateUserProfileAsync(int userId, UserProfileDto profile)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return;

            user.Username = profile.Username;
            user.Email = profile.Email;
            //user.FullName = profile.FullName;
            // Update other properties

            await _userRepository.UpdateUserAsync(user);
        }
    }
}
