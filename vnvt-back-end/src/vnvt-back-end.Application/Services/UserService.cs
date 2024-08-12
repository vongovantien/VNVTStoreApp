using AutoMapper;
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
        private readonly IMapper _mapper;
        public UserService(IUserRepository userRepository, IConfiguration configuration, IMapper mapper)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<UserDto> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAndPasswordAsync(username, password);
            if (user == null) return null;

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Token = GenerateJwtToken(user);

            return userDto;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        }),
                Expires = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:ExpirationInDay"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> CheckUserExisted(string username)
        {
            return await _userRepository.CheckUserExisted(username);
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

        public async Task<UserDto> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return null;

            return _mapper.Map<UserDto>(user); ;
        }

        public async Task UpdateUserProfileAsync(int userId, UserDto profile)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return;

            user.Username = profile.Username;
            user.Email = profile.Email;
            //user.FullName = profile.FullName;
            // Update other properties

            await _userRepository.UpdateUserAsync(user);
        }

        public async Task UploadAvatar(int userId, string url)
        {
            await _userRepository.UploadAvatar(userId, url);
        }
    }
}
