using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Infrastructure;
using vnvt_back_end.Infrastructure.Contexts;

namespace vnvt_back_end.Domain.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByUsernameAndPasswordAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckUserExisted(string username)
        {
            return await _context.Users.AnyAsync();
        }
        public async Task UploadAvatar(int userId, string url)
        {
            var user = await _context.Users.FindAsync(userId);
           // user.AvatarUrl = url;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

        }
    }
}
