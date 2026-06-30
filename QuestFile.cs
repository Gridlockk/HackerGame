using System.Text.Json;

public class QuestData
{
    public string StartServer { get; set; }
    public Dictionary<string, Server> Servers { get; set; }
    public List<Hint> Hints { get; set; }
    
    public static QuestData Load()
    {
        string json = File.ReadAllText("quest.json");

        QuestData? data = JsonSerializer.Deserialize<QuestData>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (data == null)
            throw new Exception("Не удалось загрузить quest.json");

        return data;
    }
}

public class Hint
{
    public string Text { get; set; }
    public int VisibleFromLevel { get; set; }
}
public class Server
{
    public string Address { get; set; }
    public int VisibleFromLevel { get; set; }
    public bool Locked { get; set; }

    public string? Password { get; set; }
    public string? UnlockServer { get; set; }

    public List<QuestFile> Files { get; set; }
}

public class QuestFile
{
    public string Path { get; set; }
    public string Content { get; set; }

    public int VisibleFromLevel { get; set; }

    public bool Encrypted { get; set; }
    public string? Cipher { get; set; }
    public int? Key { get; set; }
}