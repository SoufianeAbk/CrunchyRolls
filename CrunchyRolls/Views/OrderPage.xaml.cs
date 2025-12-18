using CrunchyRolls.ViewModels;

namespace CrunchyRolls.Views;

public partial class OrderPage : ContentPage
{
    private readonly OrderViewModel _viewModel;

    public OrderPage(OrderViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCart();
    }
}