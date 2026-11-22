
using Lexico.Services;

namespace Lexico.Views;

public partial class SettingsPageView : ContentPage
{
    bool _loading = true;
    bool _isUpdatingEntry;

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
        if (_loading ||
            LevelPicker.SelectedItem is not string level ||
            level == Settings.Level)
            return;

        bool ok = await DisplayAlertAsync(
            "Zmiana kursu",
            "Zmiana poziomu usunie cały postęp. Kontynuować?",
            "Tak",
            "Anuluj");

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
        if (_loading || _isUpdatingEntry)
            return;

        string input = e.NewTextValue ?? "";

        // Pozostawiamy puste pole podczas wpisywania.
        if (string.IsNullOrWhiteSpace(input))
        {
            ShowMaxError("Enter a value between 1 and 100.");
            return;
        }

        // Sprawdzamy, czy wpisano liczbę całkowitą.
        if (!int.TryParse(input, out int value))
        {
            ShowMaxError("Enter a valid whole number.");
            return;
        }

        // Walidacja zakresu.
        if (value < 1 || value > 100)
        {
            ShowMaxError("The limit must be between 1 and 100.");
            return;
        }

        // Zapisujemy tylko poprawną wartość.
        Settings.MaxPerDay = value;
        HideMaxError();
    }

    void ShowMaxError(string message)
    {
        MaxErrorLabel.Text = message;
        MaxErrorLabel.IsVisible = true;
    }

    void HideMaxError()
    {
        MaxErrorLabel.IsVisible = false;
        MaxErrorLabel.Text = "";
    }

    async void OnResetClicked(object? sender, EventArgs e)
    {
        bool ok = await DisplayAlertAsync(
            "Reset",
            "Usunąć cały postęp w kursie?",
            "Tak",
            "Anuluj");

        if (ok)
            Session.ResetProgress();
    }

    async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
