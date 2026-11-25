
using System.Windows.Input;
using Microsoft.Maui.Storage; // Udostępnia Preferences do zapisywania ulubionych.
using Lexico.Services;

namespace Lexico.ViewModels;

public class SessionPageViewModel : ViewModelBase
{
    readonly string _level;
    Session _session;

    string _word = "", _info = "", _phonetics = "", _progress = "", _summary = "";
    string _knownCount = "0", _almostCount = "0", _notKnownCount = "0";
    IReadOnlyList<string> _examples = Array.Empty<string>();
    double _progressValue;
    bool _isFlipped, _isFinished, _showStats;

    int _wordId = -1; // Przechowuje ID aktualnego słówka.
    bool _isFavorite; // Przechowuje stan ulubionego słówka.
    const string FavoritesKey = "favoriteWords"; // Określa klucz zapisywania ulubionych.

    public string Word { get => _word; set => Set(ref _word, value); }
    public string Info { get => _info; set => Set(ref _info, value); }
    public string Phonetics { get => _phonetics; set => Set(ref _phonetics, value); }
    public IReadOnlyList<string> Examples { get => _examples; set => Set(ref _examples, value); }
    public string Progress { get => _progress; set => Set(ref _progress, value); }
    public double ProgressValue { get => _progressValue; set => Set(ref _progressValue, value); }
    public string Summary { get => _summary; set => Set(ref _summary, value); }
    public string KnownCount { get => _knownCount; set => Set(ref _knownCount, value); }
    public string AlmostCount { get => _almostCount; set => Set(ref _almostCount, value); }
    public string NotKnownCount { get => _notKnownCount; set => Set(ref _notKnownCount, value); }
    public bool ShowStats { get => _showStats; set => Set(ref _showStats, value); }

    public bool IsFavorite // Udostępnia stan ulubionego słówka dla XAML.
    {
        get => _isFavorite; // Zwraca aktualny stan serca.
        set => Set(ref _isFavorite, value); // Aktualizuje stan i powiadamia interfejs.
    }

    public bool IsFlipped
    {
        get => _isFlipped;
        set { if (Set(ref _isFlipped, value)) OnPropertyChanged(nameof(IsFront)); }
    }

    public bool IsFinished
    {
        get => _isFinished;
        set { if (Set(ref _isFinished, value)) OnPropertyChanged(nameof(IsPlaying)); }
    }

    public bool IsFront => !IsFlipped;
    public bool IsPlaying => !IsFinished;

    public ICommand FlipCommand { get; }
    public ICommand RateCommand { get; }

    //krzysiek

    // Udostępnia komendę pomijania słówka.
    public ICommand SkipCommand { get; }

    public ICommand RestartCommand { get; }

    public ICommand ToggleFavoriteCommand { get; } // Udostępnia komendę przycisku serca.

    public SessionPageViewModel(string level)
    {
        _level = level;
        _session = new Session(level);

        FlipCommand = new Command(() => IsFlipped = !IsFlipped);
        RateCommand = new Command<string>(s => { _session.Rate(int.Parse(s)); Show(); });

        // Tworzy komendę wykonywaną po kliknięciu przycisku „Pomiń”.
        SkipCommand = new Command(() =>
        {
            // Pomija aktualne słówko bez wystawiania oceny.
            _session.Skip();

            // Wyświetla następne słówko.
            Show();
        });

        //krzysiek^

        ToggleFavoriteCommand = new Command(() => // Tworzy obsługę kliknięcia serca.
        {
            if (_wordId == -1) return; // Kończy działanie, jeśli nie ma aktualnego słówka.

            var favorites = GetFavoriteIds(); // Pobiera zapisane identyfikatory ulubionych.

            if (!favorites.Add(_wordId)) // Próbuje dodać aktualne słówko.
                favorites.Remove(_wordId); // Usuwa je, jeśli było już ulubione.

            Preferences.Set(FavoritesKey, string.Join(",", favorites)); // Zapisuje zmiany na urządzeniu.

            IsFavorite = favorites.Contains(_wordId); // Aktualizuje wygląd serca.
        });

        RestartCommand = new Command(async () => await StartAsync());
    }

    HashSet<int> GetFavoriteIds() // Pobiera identyfikatory ulubionych słówek.
    {
        string saved = Preferences.Get(FavoritesKey, ""); // Odczytuje zapisane dane.

        return saved.Split(',', StringSplitOptions.RemoveEmptyEntries) // Dzieli zapis na pojedyncze ID.
            .Select(id => int.TryParse(id, out int number) ? number : -1) // Zamienia tekst na liczby.
            .Where(id => id >= 0) // Odrzuca nieprawidłowe identyfikatory.
            .ToHashSet(); // Tworzy zbiór identyfikatorów.
    }

    public async Task StartAsync()
    {
        _session = new Session(_level);
        try
        {
            await _session.StartAsync();
        }
        catch (Exception ex)
        {
            Summary = "Błąd: " + ex.Message;
            ShowStats = false;
            IsFinished = true;
            return;
        }

        IsFinished = false;
        Show();
    }

    void Show()
    {
        IsFlipped = false;
        var card = _session.Current;

        if (card == null)
        {
            _wordId = -1; // Zeruje ID, gdy sesja nie ma już aktualnego słówka.
            IsFavorite = false; // Wyłącza serce po zakończeniu sesji.

            var c = _session.Counts;
            NotKnownCount = c[0].ToString();
            AlmostCount = c[1].ToString();
            KnownCount = c[2].ToString();
            ShowStats = _session.Total > 0;
            Summary = _session.Total == 0 ? "Na dziś koniec nauki" : "Koniec sesji";
            IsFinished = true;
            return;
        }

        Word = card.Value.Word;

        _wordId = card.Id; // Zapamiętuje ID wyświetlanego słówka.
        IsFavorite = GetFavoriteIds().Contains(_wordId); // Sprawdza, czy słówko jest ulubione.

        Info = $"{card.Value.Type} · {card.Value.Level}";
        Phonetics = card.Value.Phonetics.Us;
        Examples = card.Value.Examples;
        Progress = $"{_session.Position + 1}/{_session.Total}";
        ProgressValue = (double)_session.Position / _session.Total;
    }
}
