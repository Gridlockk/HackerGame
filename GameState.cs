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

    private static string GetSavePath() => 
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save.json");

    public static GameState Load()
    {
        string filePath = GetSavePath();
    
        if (!File.Exists(filePath))
        {
            var defaultState = new GameState();
            defaultState.Save(); 
            return defaultState;
        }
        try
        {
            string json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var state = JsonSerializer.Deserialize<GameState>(json, options);
        
            if (state == null)
            {
                var defaultState = new GameState();
                defaultState.Save();
                return defaultState;
            }
            return state;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки сохранения: {ex.Message}. Будет создан новый профиль.");
            var defaultState = new GameState();
            defaultState.Save();
            return defaultState;
        }
    }

    public void Save()
    {
        string filePath = GetSavePath();
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения прогресса: {ex.Message}");
        }
    }

}