using CrunchyRolls.Core.ViewModels;
using System.Diagnostics;

namespace CrunchyRolls.Views
{
    /// <summary>
    /// Code-behind voor LoginPage
    /// Behandelt pagina lifecycle en UI events
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            Debug.WriteLine("?? LoginPage geïnitialiseerd");
        }

        /// <summary>
        /// Wordt aangeroepen wanneer pagina zichtbaar wordt
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Wis gevoelige velden
            EmailEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;

            // Roep ViewModel op
            await _viewModel.OnAppearingAsync();
        }

        /// <summary>
        /// Wordt aangeroepen wanneer pagina niet meer zichtbaar is
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Wis wachtwoord veld voor veiligheid
            PasswordEntry.Text = string.Empty;
        }
    }
}