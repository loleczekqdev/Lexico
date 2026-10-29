using System.Text.Json;
using Lexico.Models;

namespace Lexico.Services;

public record LevelStats(int Total, int Learned, int Reviewed, int ToReview);

// Sesja ładuje karty dla wybranego poziomu i zapisuje ocenę każdej z nich.
// Większość kart pochodzi z wybranego poziomu, a reszta z niższych.
// Słowa oznaczone jako "Umiem" nie wracają do powtórek.
public class Session
{
    public static readonly string[] Levels = { "A1", "A2", "B1", "B2", "C1" };
    static string ProgressPath => Path.Combine(FileSystem.AppDataDirectory, "progress.json");

    readonly string _level;
    Dictionary<int, int> _progress = new();
    List<WordEntry> _cards = new();
    int _index;

    public int[] Counts { get; } = new int[3];

    public Session(string level)
    {
        _level = level;
    }

    public WordEntry? Current => _index < _cards.Count ? _cards[_index] : null;
    public int Position => _index;
    public int Total => _cards.Count;

    public async Task StartAsync()
    {
        var all = await LoadWordsAsync();
        _progress = LoadProgress();
        int lvl = Array.IndexOf(Levels, _level);

        int size = Math.Max(0, Settings.MaxPerDay - Settings.DoneToday);
        int main = (int)Math.Round(size * 0.7);

        var pool = all.Where(w =>
        {
            int l = LevelOf(w);
            return l >= 0 && l <= lvl && !(_progress.TryGetValue(w.Id, out var m) && m == 2);
        }).ToList();

        var picked = Shuffle(pool.Where(w => LevelOf(w) == lvl)).Take(main).ToList();
        picked.AddRange(Shuffle(pool.Where(w => LevelOf(w) < lvl)).Take(size - main));
        picked.AddRange(Shuffle(pool.Except(picked)).Take(size - picked.Count));

        _cards = Shuffle(picked);
        _index = 0;
    }

    public void Rate(int mastery)
    {
        var card = Current;
        if (card == null) return;

        _progress[card.Id] = mastery;
        File.WriteAllText(ProgressPath, JsonSerializer.Serialize(_progress));
        Settings.DoneToday++;
        Counts[mastery]++;
        _index++;
    }

    // Zmiana kursu / reset: kasuje cały postęp
    public static void ResetProgress()
    {
        if (File.Exists(ProgressPath)) File.Delete(ProgressPath);
        Settings.DoneToday = 0;
    }

    // Na ekranie głównym pokazujemy też słowa z niższych poziomów.
    public static async Task<LevelStats> GetStatsAsync(string level)
    {
        var all = await LoadWordsAsync();
        var progress = LoadProgress();
        int lvl = Array.IndexOf(Levels, level);

        var ids = all.Where(w => LevelOf(w) >= 0 && LevelOf(w) <= lvl).Select(w => w.Id).ToList();
        var rated = ids.Where(progress.ContainsKey).Select(id => progress[id]).ToList();

        return new LevelStats(
            Total: ids.Count,
            Learned: rated.Count(m => m == 2),
            Reviewed: rated.Count,
            ToReview: rated.Count(m => m < 2));
    }

    static int LevelOf(WordEntry w) => Array.IndexOf(Levels, w.Value.Level);

    static List<T> Shuffle<T>(IEnumerable<T> source) =>
        source.OrderBy(_ => Random.Shared.Next()).ToList();

    static Dictionary<int, int> LoadProgress()
    {
        try
        {
            if (File.Exists(ProgressPath))
                return JsonSerializer.Deserialize<Dictionary<int, int>>(File.ReadAllText(ProgressPath)) ?? new();
        }
        catch { }
        return new();
    }

    // Plik może być tablicą albo listą obiektów bez nawiasów.
    static async Task<List<WordEntry>> LoadWordsAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("words.json");
        using var reader = new StreamReader(stream);
        var text = (await reader.ReadToEndAsync()).Trim().TrimEnd(',');
        if (!text.StartsWith('[')) text = "[" + text + "]";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };
        return JsonSerializer.Deserialize<List<WordEntry>>(text, options) ?? new();
    }
}
