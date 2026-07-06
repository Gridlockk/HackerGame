namespace DexConsoleGame;

using System.Text.Json;
class GameState
{
    public string CurrentDir { get; set; } = "/";         
    public int Level { get; set; } = 1;                    
    public int Score { get; set; } = 0;                   
    public List<string> FoundKeys { get; set; } = new();  
    public HashSet<string> UnlockedFiles { get; set; } = new();
    public string CurrentServer { get; set; } = "localhost";
    
    public List<string> HackedServers { get; set; } = new();

    public static GameState Load()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save.json");
        
        if (!File.Exists(filePath))
        {
            var defaultState = new GameState();
            defaultState.Save(); 
            return defaultState;
        }
        
        string json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        return JsonSerializer.Deserialize<GameState>(json, options) ?? new GameState();
    }
    
    public void Save()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save.json");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        string json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(filePath, json);
    }

}