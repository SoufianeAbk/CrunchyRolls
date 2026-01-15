using CrunchyRolls.Models.Entities;

namespace CrunchyRolls.Data.Repositories
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        /// <summary>
        /// Get user by email address
        /// </summary>
        Task<ApplicationUser?> GetByEmailAsync(string email);

        /// <summary>
        /// Check if user exists by email
        /// </summary>
        Task<bool> UserExistsByEmailAsync(string email);

        /// <summary>
        /// Get all active users
        /// </summary>
        Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync();

        /// <summary>
        /// Update last login date for user
        /// </summary>
        Task UpdateLastLoginAsync(int userId);

        /// <summary>
        /// Verify password against stored hash
        /// </summary>
        bool VerifyPassword(string password, string hash);

        /// <summary>
        /// Hash password for storage
        /// </summary>
        string HashPassword(string password);
    }
}