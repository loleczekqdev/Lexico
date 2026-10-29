using System.Globalization;
using Lexico.Services;

namespace Lexico.ViewModels;

public class HomePageViewModel : ViewModelBase
{
    string _time = "", _date = "", _wordsLeft = "0", _courseTitle = "", _percent = "0%";
    string _reviewed = "0", _toReview = "0", _learned = "0";

    public string Time { get => _time; set => Set(ref _time, value); }
    public string Date { get => _date; set => Set(ref _date, value); }
    public string WordsLeft { get => _wordsLeft; set => Set(ref _wordsLeft, value); }
    public string CourseTitle { get => _courseTitle; set => Set(ref _courseTitle, value); }
    public string Percent { get => _percent; set => Set(ref _percent, value); }
    public string Reviewed { get => _reviewed; set => Set(ref _reviewed, value); }
    public string ToReview { get => _toReview; set => Set(ref _toReview, value); }
    public string Learned { get => _learned; set => Set(ref _learned, value); }

    public async Task RefreshAsync()
    {
        var level = Settings.Level;
        var now = DateTime.Now;
        Time = now.ToString("HH:mm");
        Date = now.ToString("d MMM", CultureInfo.InvariantCulture);
        CourseTitle = $"{level} Course Progress";

        try
        {
            var s = await Session.GetStatsAsync(level);
            int leftToday = Math.Max(0, Settings.MaxPerDay - Settings.DoneToday);
            WordsLeft = Math.Min(leftToday, s.Total - s.Learned).ToString();
            Percent = s.Total == 0 ? "0%" : $"{100 * s.Learned / s.Total}%";
            Reviewed = s.Reviewed.ToString();
            ToReview = s.ToReview.ToString();
            Learned = s.Learned.ToString();
        }
        catch
        {
            WordsLeft = "?";
        }
    }
}
