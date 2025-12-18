using CrunchyRolls.Core.ViewModels;

namespace CrunchyRolls.Views;

public partial class ProductDetailPage : ContentPage
{
    public ProductDetailPage(ProductDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}