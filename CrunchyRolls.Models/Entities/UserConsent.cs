namespace CrunchyRolls.Models.Entities
{
    /// <summary>
    /// GDPR User Consent tracking
    /// Tracks which consents user has accepted
    /// </summary>
    public class UserConsent
    {
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to User
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Privacy Policy - Data collection & processing consent
        /// </summary>
        public bool ConsentPrivacyPolicy { get; set; }

        /// <summary>
        /// Marketing & Newsletter consent
        /// </summary>
        public bool ConsentMarketing { get; set; }

        /// <summary>
        /// Cookies & Tracking consent
        /// </summary>
        public bool ConsentCookies { get; set; }

        /// <summary>
        /// Terms & Conditions acceptance
        /// </summary>
        public bool ConsentTermsConditions { get; set; }

        /// <summary>
        /// Data Processing Agreement
        /// </summary>
        public bool ConsentDataProcessing { get; set; }

        /// <summary>
        /// When consent was given
        /// </summary>
        public DateTime ConsentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When consent was last updated
        /// </summary>
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// IP address from which consent was given (for audit trail)
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent / device info
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Privacy Policy version accepted
        /// </summary>
        public string? PrivacyPolicyVersion { get; set; } = "1.0";

        /// <summary>
        /// Whether user has requested data export (GDPR Right to Data Portability)
        /// </summary>
        public bool DataExportRequested { get; set; }

        /// <summary>
        /// Whether user has requested data deletion (GDPR Right to be Forgotten)
        /// </summary>
        public bool DataDeletionRequested { get; set; }

        /// <summary>
        /// Date when deletion request was made
        /// </summary>
        public DateTime? DataDeletionRequestedDate { get; set; }
    }
}