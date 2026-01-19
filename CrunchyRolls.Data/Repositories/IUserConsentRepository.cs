using CrunchyRolls.Models.Entities;

namespace CrunchyRolls.Data.Repositories
{
    /// <summary>
    /// User Consent Repository Interface
    /// Handles GDPR consent data operations
    /// </summary>
    public interface IUserConsentRepository : IRepository<UserConsent>
    {
        /// <summary>
        /// Get consent by user ID
        /// </summary>
        Task<UserConsent?> GetByUserIdAsync(int userId);

        /// <summary>
        /// Create or update user consent
        /// </summary>
        Task<UserConsent> CreateOrUpdateAsync(UserConsent consent);

        /// <summary>
        /// Check if user has given all required consents
        /// </summary>
        Task<bool> HasRequiredConsentsAsync(int userId);

        /// <summary>
        /// Mark data deletion request
        /// </summary>
        Task<bool> RequestDataDeletionAsync(int userId);

        /// <summary>
        /// Mark data export request
        /// </summary>
        Task<bool> RequestDataExportAsync(int userId);

        /// <summary>
        /// Get all consents for audit trail
        /// </summary>
        Task<List<UserConsent>> GetAuditTrailAsync(int userId);
    }
}