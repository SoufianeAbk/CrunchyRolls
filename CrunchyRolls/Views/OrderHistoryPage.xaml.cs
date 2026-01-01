using CrunchyRolls.Core.Authentication.Interfaces;
using CrunchyRolls.Core.ViewModels;
using System.Diagnostics;

namespace CrunchyRolls.Views;

/// <summary>
/// OrderHistoryPage - Toont alle bestellingen van de ingelogde klant
/// Werkt met:
/// 1. Query parameter navigatie: //orders?email=user@example.com
/// 2. Direct UI navigatie via Orders tab (haalt email uit AuthService)
/// </summary>
public partial class OrderHistoryPage : ContentPage
{
    private readonly OrderHistoryViewModel _viewModel;

    public OrderHistoryPage(OrderHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            Debug.WriteLine("📱 OrderHistoryPage OnAppearing");

            // ✅ Haal de huidige ingelogde gebruiker op via AuthService
            // Dit wordt gebruikt als er geen email via query parameter meegegeven is
            var authService = Application.Current?.Handler.MauiContext?.Services
                .GetService<IAuthService>();

            if (authService?.CurrentUser == null)
            {
                Debug.WriteLine("❌ Geen geauthenticeerde gebruiker gevonden");
                await DisplayAlert(
                    "Fout",
                    "Je moet ingelogd zijn om je bestellingen te zien.",
                    "OK");
                return;
            }

            // ✅ Set de email uit AuthService (wordt overschreven als email via query parameter komt)
            var userEmail = authService.CurrentUser.Email;
            Debug.WriteLine($"📧 User email from AuthService: {userEmail}");

            // Als email nog niet ingesteld is (bijv. rechtstreeks op Orders tab geklikt),
            // dan zetten we die van AuthService
            if (string.IsNullOrWhiteSpace(_viewModel.CustomerEmail))
            {
                _viewModel.SetCustomerEmail(userEmail);
            }

            // ✅ Laad de bestellingen
            await _viewModel.OnAppearingAsync();

            Debug.WriteLine("✅ OrderHistoryPage loaded successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error in OrderHistoryPage.OnAppearing: {ex.Message}");
            await DisplayAlert(
                "Fout",
                $"Er is een fout opgetreden: {ex.Message}",
                "OK");
        }
    }
}