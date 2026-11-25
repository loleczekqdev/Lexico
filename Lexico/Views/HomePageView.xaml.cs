
using Lexico.Services; // Udostępnia ustawienia aplikacji.
using Lexico.ViewModels; // Udostępnia HomePageViewModel.

namespace Lexico.Views;

public partial class HomePageView : ContentPage
{
    // Przechowuje ViewModel strony głównej.
    readonly HomePageViewModel _vm;

    // Konstruktor strony głównej.
    public HomePageView()
    {
        // Wczytuje elementy z pliku XAML.
        InitializeComponent();

        // Tworzy ViewModel i przypisuje go do strony.
        BindingContext = _vm = new HomePageViewModel();
    }

    // Wywołuje się za każdym razem, gdy strona staje się widoczna.
    protected override async void OnAppearing()
    {
        // Wywołuje podstawową obsługę pojawienia się strony.
        base.OnAppearing();

        // Odświeża statystyki i listę ulubionych słówek.
        await _vm.RefreshAsync();
    }

    // Obsługuje kliknięcie przycisku rozpoczynającego naukę.
    async void OnStartClicked(object? sender, EventArgs e)
    {
        try
        {
            // Otwiera sesję nauki dla wybranego poziomu.
            await Navigation.PushAsync(new SessionPageView(Settings.Level));
        }
        catch (Exception ex)
        {
            // Wyświetla komunikat, jeśli nie uda się otworzyć sesji.
            await DisplayAlert("Błąd", ex.Message, "OK");
        }
    }

    // Obsługuje kliknięcie ustawień.
    async void OnSettingsTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            // Otwiera istniejącą stronę ustawień.
            await Navigation.PushAsync(new SettingsPageView());
        }
        catch (Exception ex)
        {
            // Wyświetla komunikat, jeśli nie uda się otworzyć ustawień.
            await DisplayAlert("Błąd", ex.Message, "OK");
        }
    }

    // Obsługuje kliknięcie przycisku ulubionych.
    async void OnFavoritesClicked(object? sender, EventArgs e)
    {
        // Ponownie pobiera zapisane ulubione słówka.
        await _vm.RefreshFavoritesAsync();

        // Zmienia widoczność listy.
        _vm.IsFavoritesVisible = !_vm.IsFavoritesVisible;
    }
}
