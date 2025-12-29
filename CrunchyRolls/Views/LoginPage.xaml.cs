using CrunchyRolls.Core.Authentication.Interfaces;
using CrunchyRolls.Core.ViewModels;
using System.Diagnostics;

namespace CrunchyRolls.Views
{
    /// <summary>
    /// LoginPage code-behind
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            // Haal AuthService op van DI container
            var authService = Application.Current?.Handler.MauiContext?.Services
                .GetService<IAuthService>();

            if (authService != null)
            {
                // Bind ViewModel aan page met service injection
                this.BindingContext = new LoginViewModel(authService);
                Debug.WriteLine("✅ LoginPage initialized with AuthService");
            }
            else
            {
                Debug.WriteLine("❌ AuthService not available in LoginPage");
            }
        }

        /// <summary>
        /// Wordt aangeroepen wanneer pagina verschijnt
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("📱 LoginPage verschenen");
        }
    }
}