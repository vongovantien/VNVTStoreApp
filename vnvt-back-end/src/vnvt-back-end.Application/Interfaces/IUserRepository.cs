using vnvt_back_end.Infrastructure;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAndPasswordAsync(string username, string password);
        Task<User> GetUserByEmailAsync(string email);
        Task CreateUserAsync(User user);
        Task<User> GetUserByIdAsync(int id);
        Task UpdateUserAsync(User user);
    }
}
