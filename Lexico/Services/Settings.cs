namespace Lexico.Services;

// Ustawienia w Preferences.
public static class Settings
{
    public static string Level
    {
        get => Preferences.Get("level", "A1");
        set => Preferences.Set("level", value);
    }

    public static int MaxPerDay
    {
        get => Preferences.Get("maxPerDay", 10);
        set => Preferences.Set("maxPerDay", value);
    }

    public static int DoneToday
    {
        get
        {
            if (Preferences.Get("doneDate", "") != Today)
            {
                Preferences.Set("doneDate", Today);
                Preferences.Set("doneToday", 0);
            }
            return Preferences.Get("doneToday", 0);
        }
        set
        {
            Preferences.Set("doneDate", Today);
            Preferences.Set("doneToday", value);
        }
    }

    static string Today => DateTime.Today.ToString("yyyy-MM-dd");
}
