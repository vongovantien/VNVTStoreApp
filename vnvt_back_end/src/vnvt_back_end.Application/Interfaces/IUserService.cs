using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> AuthenticateAsync(string username, string password);
        Task RegisterAsync(string username, string email, string password);
        Task<bool> CheckUserExisted(string username);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
        Task<UserDto> GetUserProfileAsync(int userId);
        Task UpdateUserProfileAsync(int userId, UserDto profile);
        Task UploadAvatar(int userId, string url);
        Task Logout(int userId);
        //Task<string> GetRefreshTokenAsync(string token);
    }
}
