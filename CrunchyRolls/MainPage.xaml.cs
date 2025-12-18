using CrunchyRolls.Views;

namespace CrunchyRolls;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnSushiCategoryTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProductsPage");
    }

    private async void OnRamenCategoryTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProductsPage");
    }

    private async void OnAppetisersCategoryTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProductsPage");
    }

    private async void OnStartOrderingTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProductsPage");
    }
}