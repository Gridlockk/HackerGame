using System.Text.Json;
namespace DexConsoleGame;

public class QuestData
{
    public string StartServer { get; set; }
    public Dictionary<string, Server> Servers { get; set; } = new(); 
    public List<Hint> Hints { get; set; } = new(); 
    
    public static QuestData Load()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quest.json");
    
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Файл квеста не найден: {filePath}");
        }
        try
        {
            string json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            QuestData? data = JsonSerializer.Deserialize<QuestData>(json, options);
            if (data == null)
                throw new InvalidOperationException("Не удалось десериализовать quest.json: данные равны null");
            data.Servers ??= new Dictionary<string, Server>();
            data.Hints ??= new List<Hint>();
            foreach (var server in data.Servers.Values)
                server.Files ??= new List<QuestFile>();
            return data;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка загрузки квеста: {ex.Message}", ex);
        }
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

    public List<QuestFile> Files { get; set; }  = new();
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