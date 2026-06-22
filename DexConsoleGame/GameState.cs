namespace DexConsoleGame;
class GameState
{
    public string CurrentDir = "/";          // где стоит игрок
    public int Level = 2;                     // текущий уровень сюжета
    public int Score = 0;                     // очки
    public List<string> FoundKeys = new();    // найденные ключи и пароли
    public HashSet<string> UnlockedFiles = new(); // что уже взломано и доступно
    public string CurrentServer { get; set; } = "localhost";

}