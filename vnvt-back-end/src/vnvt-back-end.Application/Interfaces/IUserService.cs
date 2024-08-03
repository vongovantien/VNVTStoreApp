using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IUserService
    {
        Task<string> AuthenticateAsync(string username, string password);
        Task RegisterAsync(string username, string email, string password);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task UpdateUserProfileAsync(int userId, UserProfileDto profile);
    }
}
