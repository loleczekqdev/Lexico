
using System.Globalization; // Obs³uga formatowania daty i godziny.
using Lexico.Services; // Dostêp do istniej¹cych us³ug aplikacji.

namespace Lexico.ViewModels; // Przestrzeñ nazw istniej¹cych ViewModeli.

public class HomePageViewModel : ViewModelBase // ViewModel ekranu g³ównego.
{
    string _time = "", _date = "", _wordsLeft = "0", _courseTitle = "", _percent = "0%"; // Przechowuje czas, datê, pozosta³e s³ówka, tytu³ i procent.
    string _reviewed = "0", _toReview = "0", _learned = "0"; // Przechowuje dotychczasowe statystyki nauki.
    double _courseProgress = 0; // Przechowuje postêp paska w zakresie od 0 do 1.

    public string Time { get => _time; set => Set(ref _time, value); } // Udostêpnia aktualn¹ godzinê.
    public string Date { get => _date; set => Set(ref _date, value); } // Udostêpnia aktualn¹ datê.
    public string WordsLeft { get => _wordsLeft; set => Set(ref _wordsLeft, value); } // Udostêpnia liczbê pozosta³ych s³ówek.
    public string CourseTitle { get => _courseTitle; set => Set(ref _courseTitle, value); } // Udostêpnia tytu³ postêpu kursu.
    public string Percent { get => _percent; set => Set(ref _percent, value); } // Udostêpnia procent opanowanych s³ówek.
    public string Reviewed { get => _reviewed; set => Set(ref _reviewed, value); } // Udostêpnia liczbê powtórzonych s³ówek.
    public string ToReview { get => _toReview; set => Set(ref _toReview, value); } // Udostêpnia liczbê s³ówek do powtórzenia.
    public string Learned { get => _learned; set => Set(ref _learned, value); } // Udostêpnia liczbê opanowanych s³ówek.
    public double CourseProgress { get => _courseProgress; set => Set(ref _courseProgress, value); } // Udostêpnia wartoœæ wype³nienia paska postêpu.

    public async Task RefreshAsync() // Odœwie¿a dane wyœwietlane na ekranie g³ównym.
    {
        var level = Settings.Level; // Pobiera aktualnie wybrany poziom jêzykowy.
        var now = DateTime.Now; // Pobiera aktualn¹ datê i godzinê.
        Time = now.ToString("HH:mm"); // Ustawia godzinê w formacie godzina:minuta.
        Date = now.ToString("d MMM", CultureInfo.InvariantCulture); // Ustawia datê w formacie dzieñ i miesi¹c.
        CourseTitle = $"{level} Course Progress"; // Ustawia tytu³ aktualnego poziomu kursu.

        try // Rozpoczyna obs³ugê pobierania statystyk.
        {
            var s = await Session.GetStatsAsync(level); // Pobiera statystyki s³ówek dla wybranego poziomu.
            int leftToday = Math.Max(0, Settings.MaxPerDay - Settings.DoneToday); // Oblicza liczbê s³ówek pozosta³ych na dzisiaj.
            WordsLeft = Math.Min(leftToday, s.Total - s.Learned).ToString(); // Ustawia liczbê s³ówek pozosta³ych do nauki dzisiaj.
            Percent = s.Total == 0 ? "0%" : $"{100 * s.Learned / s.Total}%"; // Oblicza procent opanowanych s³ówek.
            CourseProgress = s.Total == 0 ? 0 : Math.Clamp((double)s.Learned / s.Total, 0, 1); // Oblicza wype³nienie paska w zakresie od 0 do 1.
            Reviewed = s.Reviewed.ToString(); // Ustawia liczbê powtórzonych s³ówek.
            ToReview = s.ToReview.ToString(); // Ustawia liczbê s³ówek wymagaj¹cych ponownego powtórzenia.
            Learned = s.Learned.ToString(); // Ustawia liczbê opanowanych s³ówek.
        }
        catch // Obs³uguje ewentualny b³¹d pobierania statystyk.
        {
            WordsLeft = "?"; // Wyœwietla znak zapytania zamiast liczby, jeœli wyst¹pi b³¹d.
        }
    }
}
