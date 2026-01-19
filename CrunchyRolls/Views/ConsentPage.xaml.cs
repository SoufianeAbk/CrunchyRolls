using CrunchyRolls.Core.Services;
using System.Diagnostics;

namespace CrunchyRolls.Views;

public partial class ConsentPage : ContentPage
{
    private readonly GdprService _gdprService;

    public ConsentPage()
    {
        InitializeComponent();
        _gdprService = new GdprService(new ApiService());
    }

    private async void OnAcceptClicked(object sender, EventArgs e)
    {
        // Check required consents
        if (!PrivacyPolicyCheckBox.IsChecked || !TermsCheckBox.IsChecked || !DataProcessingCheckBox.IsChecked)
        {
            await DisplayAlert("⚠️ Required", "You must accept all required terms to continue", "OK");
            return;
        }

        try
        {
            AcceptButton.IsEnabled = false;

            // Save consents to backend
            var success = await _gdprService.SaveConsentAsync(
                privacyPolicy: PrivacyPolicyCheckBox.IsChecked,
                marketing: MarketingCheckBox.IsChecked,
                cookies: CookiesCheckBox.IsChecked,
                termsConditions: TermsCheckBox.IsChecked,
                dataProcessing: DataProcessingCheckBox.IsChecked);

            if (success)
            {
                Debug.WriteLine("✅ Consents saved");

                // Store consent accepted flag locally
                await SecureStorage.SetAsync("consent_accepted", "true");
                await SecureStorage.SetAsync("consent_accepted_date", DateTime.UtcNow.ToString("O"));

                // Navigate to main app
                await Shell.Current.GoToAsync("///products");
            }
            else
            {
                await DisplayAlert("Error", "Failed to save consents. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error: {ex.Message}");
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            AcceptButton.IsEnabled = true;
        }
    }

    private async void OnDeclineClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert(
            "Decline Terms?",
            "You must accept the required terms to use CrunchyRolls. You will be logged out.",
            "Decline Anyway",
            "Cancel");

        if (result)
        {
            // Log out user
            await Shell.Current.GoToAsync("///login");
        }
    }
}