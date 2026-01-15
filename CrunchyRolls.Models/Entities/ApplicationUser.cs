using Microsoft.AspNetCore.Identity;

namespace CrunchyRolls.Models.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
    }
}