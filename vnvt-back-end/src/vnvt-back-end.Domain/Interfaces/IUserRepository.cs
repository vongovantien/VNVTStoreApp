using vnvt_back_end.Infrastructure;

namespace vnvt_back_end.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAndPasswordAsync(string username, string password);
    }
}
