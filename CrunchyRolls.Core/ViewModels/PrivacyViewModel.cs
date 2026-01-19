using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrunchyRolls.Core.Services;
using System.Diagnostics;

namespace CrunchyRolls.Core.ViewModels
{
    /// <summary>
    /// Privacy & GDPR Settings ViewModel
    /// Manages user privacy settings and consent
    /// </summary>
    public partial class PrivacyViewModel : BaseViewModel
    {
        private readonly GdprService _gdprService;

        // ===== OBSERVABLE PROPERTIES =====

        [ObservableProperty]
        private string privacyPolicyText = "Loading...";

        [ObservableProperty]
        private string termsText = "Loading...";

        [ObservableProperty]
        private bool consentPrivacyPolicy;

        [ObservableProperty]
        private bool consentMarketing;

        [ObservableProperty]
        private bool consentCookies;

        [ObservableProperty]
        private bool consentTermsConditions;

        [ObservableProperty]
        private bool consentDataProcessing;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool showDeleteConfirmation;

        // ===== CONSTRUCTOR =====

        public PrivacyViewModel()
        {
            _gdprService = new GdprService(new ApiService());
            Debug.WriteLine("🔐 PrivacyViewModel initialized");
        }

        // ===== COMMANDS =====

        /// <summary>
        /// Load privacy policy & current consents
        /// </summary>
        [RelayCommand]
        public async Task LoadPrivacyDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading privacy data...";

                // Load privacy policy
                var policy = await _gdprService.GetPrivacyPolicyAsync();
                PrivacyPolicyText = policy ?? "Error loading privacy policy";

                // Load current consents
                var consent = await _gdprService.GetConsentAsync();
                if (consent != null)
                {
                    ConsentPrivacyPolicy = consent.ConsentPrivacyPolicy ?? false;
                    ConsentMarketing = consent.ConsentMarketing ?? false;
                    ConsentCookies = consent.ConsentCookies ?? false;
                    ConsentTermsConditions = consent.ConsentTermsConditions ?? false;
                    ConsentDataProcessing = consent.ConsentDataProcessing ?? false;

                    Debug.WriteLine("✅ Privacy data loaded");
                }

                StatusMessage = "Privacy data loaded";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error loading privacy data: {ex.Message}");
                StatusMessage = "Error loading privacy data";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Save consent preferences
        /// </summary>
        [RelayCommand]
        public async Task SaveConsentAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Saving preferences...";

                var success = await _gdprService.SaveConsentAsync(
                    ConsentPrivacyPolicy,
                    ConsentMarketing,
                    ConsentCookies,
                    ConsentTermsConditions,
                    ConsentDataProcessing);

                if (success)
                {
                    StatusMessage = "✅ Preferences saved successfully";
                    Debug.WriteLine("✅ Consent saved");
                }
                else
                {
                    StatusMessage = "❌ Error saving preferences";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error saving consent: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Request data export
        /// </summary>
        [RelayCommand]
        public async Task ExportDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Requesting data export...";

                var success = await _gdprService.RequestDataExportAsync();

                if (success)
                {
                    StatusMessage = "✅ Data export request submitted. Check email for download link.";
                    Debug.WriteLine("✅ Data export requested");
                }
                else
                {
                    StatusMessage = "❌ Error requesting data export";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error exporting data: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Request account deletion
        /// </summary>
        [RelayCommand]
        public async Task DeleteAccountAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Deleting account...";

                var success = await _gdprService.RequestAccountDeletionAsync(
                    "User initiated deletion from Privacy Settings");

                if (success)
                {
                    StatusMessage = "✅ Account deletion requested. Check email for confirmation.";
                    Debug.WriteLine("✅ Account deletion requested");

                    // Navigate away after 2 seconds
                    await Task.Delay(2000);
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    StatusMessage = "❌ Error deleting account";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error deleting account: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                ShowDeleteConfirmation = false;
            }
        }

        /// <summary>
        /// Show delete confirmation dialog
        /// </summary>
        [RelayCommand]
        public void ShowDeleteConfirmationDialog()
        {
            ShowDeleteConfirmation = true;
        }

        /// <summary>
        /// Close delete confirmation dialog
        /// </summary>
        [RelayCommand]
        public void CancelDeletion()
        {
            ShowDeleteConfirmation = false;
        }
    }
}