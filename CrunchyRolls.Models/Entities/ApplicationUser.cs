using Microsoft.AspNetCore.Identity;

namespace CrunchyRolls.Models.Entities
{
    /// <summary>
    /// ApplicationUser - Extends ASP.NET Core Identity User with custom properties
    /// Uses int as primary key type instead of default string
    /// </summary>
    public class ApplicationUser : IdentityUser<int>
    {
        // ===== IDENTITY GEEFT AL =====
        // - Id (int)
        // - UserName (string)
        // - Email (string)
        // - EmailConfirmed (bool)
        // - PasswordHash (string)
        // - SecurityStamp (string)
        // - PhoneNumber (string)
        // - PhoneNumberConfirmed (bool)
        // - TwoFactorEnabled (bool)
        // - LockoutEnd (DateTimeOffset?)
        // - LockoutEnabled (bool)
        // - AccessFailedCount (int)

        // ===== CUSTOM PROPERTIES =====

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        // ===== COMPUTED PROPERTIES =====

        public string FullName => $"{FirstName} {LastName}";

        // ===== NAVIGATION PROPERTIES =====

        /// <summary>
        /// Orders placed by this user
        /// </summary>
        public virtual ICollection<Order>? Orders { get; set; }
    }
}