using Microsoft.AspNetCore.Identity;
namespace CrunchyRolls.Models.Entities
{
    public class User : IdentityUser<int>
    {
        // Identity properties inherited:
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; } = true;

        // Additional properties
        public string Role { get; set; } = "Customer";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }

        // Optional: Full name property
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}