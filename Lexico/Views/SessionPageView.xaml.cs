using Lexico.ViewModels;

namespace Lexico.Views;

public partial class SessionPageView : ContentPage
{
    readonly SessionPageViewModel _vm;

    public SessionPageView(string level)
    {
        InitializeComponent();
        BindingContext = _vm = new SessionPageViewModel(level);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.StartAsync();
    }

    async void OnBack(object? sender, EventArgs e) => await Navigation.PopAsync();
}
