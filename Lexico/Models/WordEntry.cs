namespace Lexico.Models;

// Pojedynczy wpis z words.json.
public class WordEntry
{
    public int Id { get; set; }
    public WordData Value { get; set; } = new();
}

public class WordData
{
    public string Word { get; set; } = "";
    public string Type { get; set; } = "";
    public string Level { get; set; } = "";
    public PhoneticsData Phonetics { get; set; } = new();
    public List<string> Examples { get; set; } = new();
}

public class PhoneticsData
{
    public string Us { get; set; } = "";
}
