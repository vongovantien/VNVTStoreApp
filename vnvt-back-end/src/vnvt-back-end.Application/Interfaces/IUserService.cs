using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IUserService
    {
        Task<string> AuthenticateAsync(string username, string password);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
        Task<UserDto> GetUserProfileAsync(int userId);
        Task UpdateUserProfileAsync(int userId, UserDto profile);
    }
}
