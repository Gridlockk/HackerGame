using System.Text.Json;

namespace DexConsoleGame;

public static class ConfigLoader
{
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