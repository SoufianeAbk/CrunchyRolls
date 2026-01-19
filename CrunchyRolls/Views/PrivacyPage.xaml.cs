using CrunchyRolls.Core.ViewModels;

namespace CrunchyRolls.Views;

public partial class PrivacyPage : ContentPage
{
    public PrivacyPage()
    {
        InitializeComponent();
        BindingContext = new PrivacyViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is PrivacyViewModel viewModel)
        {
            await viewModel.LoadPrivacyDataCommand.ExecuteAsync(null);
        }
    }
}