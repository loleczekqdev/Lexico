using Lexico.Services;

namespace Lexico.Views;

public partial class SettingsPageView : ContentPage
{
    bool _loading = true;

    public SettingsPageView()
    {
        InitializeComponent();
        LevelPicker.ItemsSource = Session.Levels;
        LevelPicker.SelectedItem = Settings.Level;
        MaxEntry.Text = Settings.MaxPerDay.ToString();
        _loading = false;
    }

    // zmiana kursu kasuje cały postęp (po potwierdzeniu)
    async void OnLevelChanged(object? sender, EventArgs e)
    {
        if (_loading || LevelPicker.SelectedItem is not string level || level == Settings.Level) return;

        bool ok = await DisplayAlertAsync("Zmiana kursu", "Zmiana poziomu usunie cały postęp. Kontynuować?", "Tak", "Anuluj");
        if (ok)
        {
            Settings.Level = level;
            Session.ResetProgress();
        }
        else
        {
            _loading = true;
            LevelPicker.SelectedItem = Settings.Level;
            _loading = false;
        }
    }

    void OnMaxChanged(object? sender, TextChangedEventArgs e)
    {
        if (int.TryParse(e.NewTextValue, out var n) && n > 0)
            Settings.MaxPerDay = Math.Min(n, 500);
    }

    async void OnResetClicked(object? sender, EventArgs e)
    {
        bool ok = await DisplayAlertAsync("Reset", "Usunąć cały postęp w kursie?", "Tak", "Anuluj");
        if (ok) Session.ResetProgress();
    }

    async void OnBackTapped(object? sender, TappedEventArgs e) => await Navigation.PopAsync();
}
