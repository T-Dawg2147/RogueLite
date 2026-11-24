using System.Text.Json;
using System.Text.Json.Serialization;

namespace RogueLite;

public class SaveData
{
    public int Gold { get; set; }
    public int Character { get; set; }
    public int HealthLevel { get; set; }
    public int BestFloor { get; set; }

    [JsonIgnore] public int MaxHp => 5 + HealthLevel;
}

public static class Save
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "save.json");

    public static SaveData Data = new();

    public static void Load()
    {
        try
        {
            if (File.Exists(FilePath))
                Data = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(FilePath)) ?? new SaveData();
        }
        catch
        {
            Data = new SaveData();
        }
    }

    public static void Write() => File.WriteAllText(FilePath, JsonSerializer.Serialize(Data));
}