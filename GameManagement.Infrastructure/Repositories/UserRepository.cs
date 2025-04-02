using Microsoft.EntityFrameworkCore;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Interfaces;
using GameManagement.Infrastructure.Persistence;

namespace GameManagement.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly GameManagementDbContext _context;

        public UserRepository(GameManagementDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<bool> ExistsAsync(string email, string username)
        {
            return await _context.Users.AnyAsync(u =>
                u.Email.ToLower() == email.ToLower() ||
                u.Username.ToLower() == username.ToLower());
        }

        public async Task CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}