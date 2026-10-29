using System.Windows.Input;
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
    public ICommand RestartCommand { get; }

    public SessionPageViewModel(string level)
    {
        _level = level;
        _session = new Session(level);

        FlipCommand = new Command(() => IsFlipped = !IsFlipped);
        RateCommand = new Command<string>(s => { _session.Rate(int.Parse(s)); Show(); });
        RestartCommand = new Command(async () => await StartAsync());
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
        Info = $"{card.Value.Type} · {card.Value.Level}";
        Phonetics = card.Value.Phonetics.Us;
        Examples = card.Value.Examples;
        Progress = $"{_session.Position + 1}/{_session.Total}";
        ProgressValue = (double)_session.Position / _session.Total;
    }
}
