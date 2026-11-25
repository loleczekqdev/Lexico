
using System.Collections.ObjectModel; // Udostêpnia kolekcjê s³ówek, która automatycznie odœwie¿a widok.
using System.Globalization; // Umo¿liwia formatowanie daty.
using System.Text.Json; // Umo¿liwia odczytywanie danych z pliku JSON.
using Lexico.Models; // Udostêpnia model s³ówka WordEntry.
using Lexico.Services; // Udostêpnia Session i Settings.
using Microsoft.Maui.Storage; // Udostêpnia Preferences.

namespace Lexico.ViewModels;

public class HomePageViewModel : ViewModelBase
{
    // Przechowuje wartoœci wyœwietlane na ekranie g³ównym.
    string _time = "", _date = "", _wordsLeft = "0", _courseTitle = "", _percent = "0%";
    string _reviewed = "0", _toReview = "0", _learned = "0";

    // Przechowuje informacjê o widocznoœci listy ulubionych.
    bool _isFavoritesVisible;

    // Przechowuje tekst przycisku ulubionych.
    string _favoritesButtonText = "Poka¿ ulubione";

    // Przechowuje postêp kursu w formie wartoœci od 0 do 1.
    double _courseProgress;

    // Klucz, pod którym zapisujemy ulubione s³ówka.
    const string FavoritesKey = "favoriteWords";

    // Kolekcja s³ówek oznaczonych jako ulubione.
    public ObservableCollection<WordEntry> FavoriteWords { get; } = new();

    // W³aœciwoœci powi¹zane z elementami XAML.
    public string Time { get => _time; set => Set(ref _time, value); }
    public string Date { get => _date; set => Set(ref _date, value); }
    public string WordsLeft { get => _wordsLeft; set => Set(ref _wordsLeft, value); }
    public string CourseTitle { get => _courseTitle; set => Set(ref _courseTitle, value); }
    public string Percent { get => _percent; set => Set(ref _percent, value); }
    public string Reviewed { get => _reviewed; set => Set(ref _reviewed, value); }
    public string ToReview { get => _toReview; set => Set(ref _toReview, value); }
    public string Learned { get => _learned; set => Set(ref _learned, value); }

    // Wartoœæ wykorzystywana przez pasek postêpu.
    public double CourseProgress
    {
        get => _courseProgress;
        set => Set(ref _courseProgress, value);
    }

    // Okreœla, czy lista ulubionych jest aktualnie widoczna.
    public bool IsFavoritesVisible
    {
        get => _isFavoritesVisible;
        set => Set(ref _isFavoritesVisible, value);
    }

    // Tekst przycisku pokazuje równie¿ liczbê ulubionych s³ówek.
    public string FavoritesButtonText
    {
        get => _favoritesButtonText;
        set => Set(ref _favoritesButtonText, value);
    }

    // Odœwie¿a dane wyœwietlane na ekranie g³ównym.
    public async Task RefreshAsync()
    {
        // Pobiera aktualny poziom kursu.
        var level = Settings.Level;

        // Pobiera aktualn¹ datê i godzinê.
        var now = DateTime.Now;

        // Ustawia godzinê.
        Time = now.ToString("HH:mm");

        // Ustawia datê.
        Date = now.ToString("d MMM", CultureInfo.InvariantCulture);

        // Ustawia nazwê kursu.
        CourseTitle = $"{level} Course Progress";

        try
        {
            // Pobiera statystyki nauki dla wybranego poziomu.
            var s = await Session.GetStatsAsync(level);

            // Oblicza liczbê s³ówek pozosta³ych na dzisiaj.
            int leftToday = Math.Max(0, Settings.MaxPerDay - Settings.DoneToday);

            // Ustawia liczbê s³ówek pozosta³ych do nauki.
            WordsLeft = Math.Min(leftToday, Math.Max(0, s.Total - s.Learned)).ToString();

            // Oblicza procent ukoñczenia kursu.
            int percent = s.Total == 0 ? 0 : (int)Math.Round(100.0 * s.Learned / s.Total);

            // Wyœwietla procent ukoñczenia.
            Percent = $"{percent}%";

            // Ustawia wartoœæ paska postêpu od 0 do 1.
            CourseProgress = s.Total == 0 ? 0 : (double)s.Learned / s.Total;

            // Ustawia liczbê powtórzonych s³ówek.
            Reviewed = s.Reviewed.ToString();

            // Ustawia liczbê s³ówek do ponownej nauki.
            ToReview = s.ToReview.ToString();

            // Ustawia liczbê opanowanych s³ówek.
            Learned = s.Learned.ToString();
        }
        catch
        {
            // Wyœwietla znak zapytania, jeœli nie uda siê pobraæ statystyk.
            WordsLeft = "?";
        }

        // Odœwie¿a listê ulubionych s³ówek.
        await RefreshFavoritesAsync();
    }

    // Wczytuje zapisane ulubione s³ówka.
    public async Task RefreshFavoritesAsync()
    {
        try
        {
            // Pobiera zapisane identyfikatory ulubionych s³ówek.
            string saved = Preferences.Get(FavoritesKey, "");

            // Zamienia zapisane identyfikatory na zbiór liczb.
            var favoriteIds = saved
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out int number) ? number : -1)
                .Where(id => id >= 0)
                .ToHashSet();

            // Usuwa poprzedni¹ zawartoœæ listy.
            FavoriteWords.Clear();

            // Jeœli nie ma ulubionych, ustawia odpowiedni tekst przycisku.
            if (favoriteIds.Count == 0)
            {
                FavoritesButtonText = "Poka¿ ulubione (0)";
                return;
            }

            // Otwiera istniej¹cy plik words.json z zasobów aplikacji.
            using var stream = await FileSystem.OpenAppPackageFileAsync("words.json");

            // Odczytuje zawartoœæ pliku.
            using var reader = new StreamReader(stream);

            // Pobiera tekst z pliku i usuwa koñcowy przecinek.
            var text = (await reader.ReadToEndAsync()).Trim().TrimEnd(',');

            // Dodaje nawiasy tablicy, jeœli plik ich nie zawiera.
            if (!text.StartsWith('['))
                text = "[" + text + "]";

            // Ustawia opcje odczytywania JSON.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            // Odczytuje wszystkie s³ówka z pliku.
            var allWords = JsonSerializer.Deserialize<List<WordEntry>>(text, options) ?? new();

            // Wybiera tylko s³ówka, których identyfikatory zapisano jako ulubione.
            var favorites = allWords
                .Where(word => favoriteIds.Contains(word.Id))
                .OrderBy(word => word.Value.Word)
                .ToList();

            // Dodaje znalezione s³ówka do kolekcji wyœwietlanej w XAML.
            foreach (var word in favorites)
                FavoriteWords.Add(word);

            // Aktualizuje tekst przycisku.
            FavoritesButtonText = $"Poka¿ ulubione ({FavoriteWords.Count})";
        }
        catch
        {
            // W razie b³êdu czyœci listê i informuje o braku wyników.
            FavoriteWords.Clear();
            FavoritesButtonText = "Poka¿ ulubione (0)";
        }
    }
}
