using CrunchyRolls.Data.Context;
using CrunchyRolls.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CrunchyRolls.Data.Repositories
{
    /// <summary>
    /// User Consent Repository Implementation
    /// Handles GDPR consent data persistence
    /// </summary>
    public class UserConsentRepository : Repository<UserConsent>, IUserConsentRepository
    {
        public UserConsentRepository(ApplicationDbContext context) : base(context)
        {
            Debug.WriteLine("🔐 UserConsentRepository initialized");
        }

        /// <summary>
        /// Get consent by user ID
        /// </summary>
        public async Task<UserConsent?> GetByUserIdAsync(int userId)
        {
            try
            {
                var consent = await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
                return consent;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting consent for user {userId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create or update user consent
        /// </summary>
        public async Task<UserConsent> CreateOrUpdateAsync(UserConsent consent)
        {
            try
            {
                var existing = await GetByUserIdAsync(consent.UserId);

                if (existing != null)
                {
                    // Update existing consent
                    existing.ConsentPrivacyPolicy = consent.ConsentPrivacyPolicy;
                    existing.ConsentMarketing = consent.ConsentMarketing;
                    existing.ConsentCookies = consent.ConsentCookies;
                    existing.ConsentTermsConditions = consent.ConsentTermsConditions;
                    existing.ConsentDataProcessing = consent.ConsentDataProcessing;
                    existing.LastUpdated = DateTime.UtcNow;
                    existing.IpAddress = consent.IpAddress;
                    existing.UserAgent = consent.UserAgent;

                    await UpdateAsync(existing);
                    Debug.WriteLine($"✅ Consent updated for user {consent.UserId}");
                    return existing;
                }
                else
                {
                    // Create new consent
                    consent.ConsentDate = DateTime.UtcNow;
                    await AddAsync(consent);
                    Debug.WriteLine($"✅ Consent created for user {consent.UserId}");
                    return consent;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error creating/updating consent: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if user has given all required consents
        /// </summary>
        public async Task<bool> HasRequiredConsentsAsync(int userId)
        {
            try
            {
                var consent = await GetByUserIdAsync(userId);

                if (consent == null)
                    return false;

                // Required: Privacy Policy & Terms Conditions
                return consent.ConsentPrivacyPolicy && consent.ConsentTermsConditions;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error checking required consents: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Mark data deletion request
        /// </summary>
        public async Task<bool> RequestDataDeletionAsync(int userId)
        {
            try
            {
                var consent = await GetByUserIdAsync(userId);

                if (consent == null)
                    return false;

                consent.DataDeletionRequested = true;
                consent.DataDeletionRequestedDate = DateTime.UtcNow;
                await UpdateAsync(consent);

                Debug.WriteLine($"✅ Data deletion requested for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error requesting data deletion: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Mark data export request
        /// </summary>
        public async Task<bool> RequestDataExportAsync(int userId)
        {
            try
            {
                var consent = await GetByUserIdAsync(userId);

                if (consent == null)
                {
                    // Create consent record if doesn't exist
                    var newConsent = new UserConsent
                    {
                        UserId = userId,
                        DataExportRequested = true,
                        ConsentDate = DateTime.UtcNow
                    };
                    await AddAsync(newConsent);
                }
                else
                {
                    consent.DataExportRequested = true;
                    await UpdateAsync(consent);
                }

                Debug.WriteLine($"✅ Data export requested for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error requesting data export: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get all consents for audit trail
        /// </summary>
        public async Task<List<UserConsent>> GetAuditTrailAsync(int userId)
        {
            try
            {
                var consents = await _dbSet
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.ConsentDate)
                    .ToListAsync();

                return consents;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting audit trail: {ex.Message}");
                return new List<UserConsent>();
            }
        }
    }
}