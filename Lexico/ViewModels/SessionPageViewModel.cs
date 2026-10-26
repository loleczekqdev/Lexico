using System.Windows.Input;
using Lexico.Services;

namespace Lexico.ViewModels;

public class SessionPageViewModel : ViewModelBase
{
    readonly string _level;
    Session _session;

    string _word = "", _info = "", _phonetics = "", _examples = "", _progress = "", _summary = "";
    bool _isFlipped, _isFinished;

    public string Word { get => _word; set => Set(ref _word, value); }
    public string Info { get => _info; set => Set(ref _info, value); }
    public string Phonetics { get => _phonetics; set => Set(ref _phonetics, value); }
    public string Examples { get => _examples; set => Set(ref _examples, value); }
    public string Progress { get => _progress; set => Set(ref _progress, value); }
    public string Summary { get => _summary; set => Set(ref _summary, value); }

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
    public ICommand RateCommand { get; }      // parametr: "0" Nie umiem, "1" Prawie, "2" Umiem
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
            Summary = "Błąd: " + ex.Message;   // zamiast crasha pokaże, co poszło nie tak
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
            Summary = _session.Total == 0
                ? "Na dziś koniec nauki"
                : $"Koniec!\nNie umiem: {c[0]}\nPrawie: {c[1]}\nUmiem: {c[2]}";
            IsFinished = true;
            return;
        }

        Word = card.Value.Word;
        Info = $"{card.Value.Type} · {card.Value.Level}";
        Phonetics = card.Value.Phonetics.Us;
        Examples = string.Join("\n\n", card.Value.Examples);
        Progress = $"{_session.Position + 1}/{_session.Total}";
    }
}
