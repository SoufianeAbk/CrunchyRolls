using CrunchyRolls.Data.Context;
using CrunchyRolls.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrunchyRolls.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower().Equals(email.ToLower()));
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _dbSet
                .AnyAsync(u => u.Email.ToLower().Equals(email.ToLower()));
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.LastLoginDate = DateTime.UtcNow;
                await UpdateAsync(user);
            }
        }

        /// <summary>
        /// Verify password against stored bcrypt hash
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Hash password using bcrypt (strong hashing)
        /// </summary>
        public string HashPassword(string password)
        {
            // BCrypt automatically handles salt + hashing
            // Using cost factor of 11 (default is 10, balance between security and speed)
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
        }
    }
}