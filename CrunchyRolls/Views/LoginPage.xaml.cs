using CrunchyRolls.Core.ViewModels;

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

            // Bind ViewModel aan page
            this.BindingContext = new LoginViewModel();
        }

        /// <summary>
        /// Wordt aangeroepen wanneer pagina verschijnt
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("?? LoginPage verschenen");
        }
    }
}