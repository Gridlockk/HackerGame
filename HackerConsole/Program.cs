namespace DexConsoleGame;


class Program
{
    
    
    static void Main(string[] args)
    {
        Console.Title = "Hacker Console Game";
        Game game = new Game();
        game.Run();

    }
}