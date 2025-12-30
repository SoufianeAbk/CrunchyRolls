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
        private readonly IAuthService _authService;

        // ✅ AuthService wordt geïnjecteerd door DI container
        public LoginPage(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            this.BindingContext = new LoginViewModel(_authService);
            Debug.WriteLine("✅ LoginPage initialized");
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