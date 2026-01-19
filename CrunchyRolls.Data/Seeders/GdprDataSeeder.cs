using CrunchyRolls.Data.Context;
using CrunchyRolls.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CrunchyRolls.Data.Seeders
{
    /// <summary>
    /// GDPR Data Seeder
    /// Creates default user consents for test users
    /// </summary>
    public static class GdprDataSeeder
    {
        /// <summary>
        /// Seed user consents for all users
        /// </summary>
        public static async Task SeedUserConsentsAsync(ApplicationDbContext context)
        {
            try
            {
                // Check if consents already exist
                if (await context.Set<UserConsent>().AnyAsync())
                {
                    Debug.WriteLine("ℹ️ User consents already seeded");
                    return;
                }

                Debug.WriteLine("🌱 Seeding user consents...");

                // Get all users
                var users = await context.Users.ToListAsync();

                var consents = new List<UserConsent>();

                foreach (var user in users)
                {
                    var consent = new UserConsent
                    {
                        UserId = user.Id,
                        ConsentPrivacyPolicy = true,        // ✅ Required
                        ConsentMarketing = false,            // ❌ Opt-in
                        ConsentCookies = true,               // ✅ Recommended
                        ConsentTermsConditions = true,       // ✅ Required
                        ConsentDataProcessing = true,        // ✅ Required
                        ConsentDate = DateTime.UtcNow,
                        IpAddress = "127.0.0.1",
                        UserAgent = "Seeded consent",
                        PrivacyPolicyVersion = "1.0"
                    };

                    consents.Add(consent);
                    Debug.WriteLine($"  ✅ Created consent for user {user.Email}");
                }

                await context.Set<UserConsent>().AddRangeAsync(consents);
                await context.SaveChangesAsync();

                Debug.WriteLine($"✅ Seeded {consents.Count} user consents");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error seeding user consents: {ex.Message}");
                throw;
            }
        }
    }
}