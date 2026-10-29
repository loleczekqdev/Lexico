using Lexico.Services;
using Lexico.ViewModels;

namespace Lexico.Views;

public partial class HomePageView : ContentPage
{
    readonly HomePageViewModel _vm;

    public HomePageView()
    {
        InitializeComponent();
        BindingContext = _vm = new HomePageViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.RefreshAsync();
    }

    async void OnStartClicked(object? sender, EventArgs e)
    {
        try { await Navigation.PushAsync(new SessionPageView(Settings.Level)); }
        catch (Exception ex) { await DisplayAlert("Błąd", ex.Message, "OK"); }
    }

    async void OnSettingsTapped(object? sender, TappedEventArgs e)
    {
        try { await Navigation.PushAsync(new SettingsPageView()); }
        catch (Exception ex) { await DisplayAlert("Błąd", ex.Message, "OK"); }
    }
}
