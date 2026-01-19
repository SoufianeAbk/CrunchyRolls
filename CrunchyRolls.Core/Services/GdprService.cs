using System.Diagnostics;

namespace CrunchyRolls.Core.Services
{
    /// <summary>
    /// GDPR Compliance Service
    /// Handles user data export, deletion, and consent management
    /// </summary>
    public class GdprService
    {
        private readonly ApiService _apiService;

        public GdprService(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            Debug.WriteLine("🔐 GdprService initialized");
        }

        // ===== CONSENT MANAGEMENT =====

        /// <summary>
        /// Save user consent preferences
        /// </summary>
        public async Task<bool> SaveConsentAsync(
            bool privacyPolicy,
            bool marketing,
            bool cookies,
            bool termsConditions,
            bool dataProcessing)
        {
            try
            {
                var consentData = new
                {
                    ConsentPrivacyPolicy = privacyPolicy,
                    ConsentMarketing = marketing,
                    ConsentCookies = cookies,
                    ConsentTermsConditions = termsConditions,
                    ConsentDataProcessing = dataProcessing
                };

                var result = await _apiService.PostAsync<object, bool>("privacy/consent", consentData);

                if (result)
                {
                    Debug.WriteLine("✅ User consent saved");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error saving consent: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get user's current consents
        /// </summary>
        public async Task<dynamic?> GetConsentAsync()
        {
            try
            {
                var consent = await _apiService.GetAsync<dynamic>("privacy/consent");

                if (consent != null)
                {
                    Debug.WriteLine("✅ User consent retrieved");
                }

                return consent;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error retrieving consent: {ex.Message}");
                return null;
            }
        }

        // ===== DATA EXPORT (Right to Portability) =====

        /// <summary>
        /// Request user data export (GDPR Art. 20)
        /// Returns all personal data in structured format (JSON)
        /// </summary>
        public async Task<bool> RequestDataExportAsync()
        {
            try
            {
                Debug.WriteLine("📤 Requesting data export (GDPR Art. 20)...");

                var result = await _apiService.PostAsync<object, bool>("privacy/export-data", new { });

                if (result)
                {
                    Debug.WriteLine("✅ Data export request submitted");
                    Debug.WriteLine("📧 Check your email within 30 days for download link");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error requesting data export: {ex.Message}");
                return false;
            }
        }

        // ===== DATA DELETION (Right to be Forgotten) =====

        /// <summary>
        /// Request account deletion (GDPR Art. 17)
        /// Note: Order data is kept for 7 years (legal requirement)
        /// </summary>
        public async Task<bool> RequestAccountDeletionAsync(string reason = "")
        {
            try
            {
                Debug.WriteLine("🗑️ Requesting account deletion (GDPR Art. 17)...");

                var deletionRequest = new
                {
                    Reason = reason,
                    RequestedDate = DateTime.UtcNow
                };

                var result = await _apiService.PostAsync<object, bool>("privacy/delete-account", deletionRequest);

                if (result)
                {
                    Debug.WriteLine("✅ Account deletion request submitted");
                    Debug.WriteLine("⚠️ Please note:");
                    Debug.WriteLine("   - Account will be marked for deletion");
                    Debug.WriteLine("   - Order history kept for 7 years (legal requirement)");
                    Debug.WriteLine("   - Personal data will be anonymized");
                    Debug.WriteLine("   - Confirmation email sent");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error requesting account deletion: {ex.Message}");
                return false;
            }
        }

        // ===== PRIVACY POLICY & TERMS =====

        /// <summary>
        /// Get Privacy Policy
        /// </summary>
        public async Task<string?> GetPrivacyPolicyAsync()
        {
            try
            {
                var policy = await _apiService.GetAsync<string>("privacy/policy");

                if (!string.IsNullOrEmpty(policy))
                {
                    Debug.WriteLine("✅ Privacy policy retrieved");
                }

                return policy;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error retrieving privacy policy: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Terms & Conditions
        /// </summary>
        public async Task<string?> GetTermsConditionsAsync()
        {
            try
            {
                var terms = await _apiService.GetAsync<string>("privacy/terms");

                if (!string.IsNullOrEmpty(terms))
                {
                    Debug.WriteLine("✅ Terms & conditions retrieved");
                }

                return terms;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error retrieving terms: {ex.Message}");
                return null;
            }
        }
    }
}