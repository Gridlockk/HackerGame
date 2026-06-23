using System.Reflection.Metadata.Ecma335;

namespace DexConsoleGame;
using Spectre.Console;
using System.Text.Json;

public class Game
{



    private QuestData data;

    private bool running = false;
    private GameState state;

    public Game()
    {
        data = QuestData.Load();
        state = GameState.Load();
    }


    public void Run()
    {
        WelcomeMessage();

        running = true;
        while (true)
        {
            AnsiConsole.Markup($"\n[green]root@{state.CurrentServer} > [/] ");

            string input = (Console.ReadLine() ?? "").Trim(); // ?? на случай пустого ввода

            // делим только на 2 части: команда и весь остаток как аргумент
            string[] parts = input.Split(' ', 2);
            string command = parts[0].ToLower(); // регистр не должен мешать
            string arg = parts.Length > 1 ? parts[1] : "";
            executeCommand(command, arg);
            state.Save();
        }
    }


    static void SlowInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty");
        }

        Console.ForegroundColor = ConsoleColor.Green;

        foreach (var symbol in input)
        {
            Console.Write(symbol);
            Thread.Sleep(1);
        }

        Console.ResetColor();

    }

    private void WelcomeMessage()
    {
        AnsiConsole.Status().Spinner(Spinner.Known.Dots).SpinnerStyle(Style.Parse("green"))
            .Start("[green]Booting HackOS ...", ctx =>
            {
                ctx.Status("Booting HackOS...");
                Thread.Sleep(1500);
                ctx.Status("Connecting...");
                Thread.Sleep(1000);
                ctx.Status("Loading drivers...");
                Thread.Sleep(1000);
            });
        SlowInput(
            "Welcome, operator.\n\n" +
            "Your task is to recover the stolen data and trace its source.\n\n" +
            "Type \"help\" to view available commands.\n\n");
    }

    private void Connect(string arg)
    {


        var relative = data.Servers.FirstOrDefault(serverName => serverName.Value.Address == arg);
        if (relative.Value == null)
        {
            AnsiConsole.Markup($"\n[red] Failed connection! [/] ");
            return;
        }


        state.CurrentDir = "/";
        state.CurrentServer = relative.Key;


        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var task = ctx.AddTask("Connecting....", maxValue: 100);

                while (!ctx.IsFinished)
                {
                    task.Increment(1);

                    Thread.Sleep(10);
                }
            });
    }

    private Server GetCurrentServer()
    {
        return data.Servers[state.CurrentServer];
    }

    private void ListFiles()
    {
        var files = GetCurrentServer()
            .Files
            .Where(f => f.VisibleFromLevel <= state.Level);

        HashSet<string> folders = new();

        foreach (var file in files)
        {
            string relative;

            if (state.CurrentDir == "/")
            {
                relative = file.Path.TrimStart('/');
            }
            else
            {

                if (!file.Path.StartsWith(state.CurrentDir + "/"))
                    continue;

                relative = file.Path.Substring(state.CurrentDir.Length + 1);
            }

            string[] parts = relative.Split('/');

            if (parts.Length == 1)
            {
                // файл текущей папки
                if (file.Encrypted)
                    AnsiConsole.MarkupLine($"[red][[ENC]] {parts[0]}[/]");
                else
                    AnsiConsole.MarkupLine(parts[0]);
            }
            else
            {
                // папка
                folders.Add(parts[0]);
            }
        }

        foreach (var folder in folders)
        {
            AnsiConsole.MarkupLine($"[blue]{folder}/[/]");
        }
    }

    private void ChangeDir(string arg)
    {
        //путь не имеет вид / { /home/pol/oplolon/  }
        if (arg == "..")
        {
            if (state.CurrentDir == "/")
                return;



            int lastSlash = state.CurrentDir.TrimEnd('/').LastIndexOf('/');

            if (lastSlash <= 0)
            {
                state.CurrentDir = "/";
                return;
            }

            state.CurrentDir = state.CurrentDir.Substring(0, lastSlash);

            if (state.CurrentDir == "")
                state.CurrentDir = "/";
        }

        var files = GetCurrentServer()
            .Files
            .Where(f => f.VisibleFromLevel <= state.Level);

        HashSet<string> folders = new();


        // /home/notes.txt     /getc/34.txt
        // проверка есть ли вооьще такая папка на этом уровне


        foreach (var file in files)
        {
            string relative;

            if (state.CurrentDir == "/")
            {
                relative = file.Path.TrimStart('/');
            }
            else
            {
                if (!file.Path.StartsWith(state.CurrentDir + "/"))
                    continue;

                relative = file.Path.Substring(state.CurrentDir.Length + 1);
            }

            string[] parts = relative.Split('/');
            // home/  notex.txt 
            if (parts.Length > 1)
            {
                folders.Add(parts[0]);
            }

        }

        if (folders.Contains(arg))
        {
            if (arg.StartsWith("/"))
                state.CurrentDir = arg;
            else
                state.CurrentDir = "/" + arg;

        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Path unknown: {arg}[/]");
            return;
        }

    }

    private void OpenFile(string arg)
    {
        //  /home/rety.txt    /home/logs/log.txt    /home/test/test.txt
        var files = GetCurrentServer()
            .Files
            .Where(f => f.VisibleFromLevel <= state.Level);

        foreach (var file in files)
        {
            string relative;

            if (state.CurrentDir == "/")
            {
                relative = file.Path.TrimStart('/');
            }
            else
            {

                if (!file.Path.StartsWith(state.CurrentDir + "/"))
                    continue;

                relative = file.Path.Substring(state.CurrentDir.Length + 1);
            }

            string[] parts = relative.Split('/');

            if (parts.Length == 1 && parts[0] == arg)
            {
                if (file.Encrypted)
                {
                    if (state.UnlockedFiles.Contains(file.Path))
                        AnsiConsole.MarkupLine($"[blue]{file.Content}[/]");
                    else
                        AnsiConsole.MarkupLine($"[red]File encrypted!!! [/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[blue]{file.Content}[/]");
                }

                return;
            }
        }
    }

    private void Decrypt(string arg)
    {
        var files = GetCurrentServer().Files.Where(file => file.VisibleFromLevel <= state.Level);

        HashSet<string> folders = new();

        foreach (var file in files)
        {
            string relative;

            if (state.CurrentDir == "/")
            {
                relative = file.Path.TrimStart('/');
            }
            else
            {

                if (!file.Path.StartsWith(state.CurrentDir + "/"))
                    continue;

                relative = file.Path.Substring(state.CurrentDir.Length + 1);
            }

            string[] parts = relative.Split('/');

            if (parts.Length == 1 && parts[0] == arg)
            {
                AnsiConsole.Markup($"[green]Input Key > [/]");
                string decryptedText = Cipher.Decrypt(file.Content, int.Parse(Console.ReadLine()));
                state.UnlockedFiles.Add(file.Path);
                state.Level++;
                CheckLevelUp();
                AnsiConsole.MarkupLine($"[blue] {decryptedText} [/]");
                return;

            }
        }

        AnsiConsole.MarkupLine($"[red]File not found [/]");

    }

    private void ShowStatus()
    {


        Console.WriteLine("\n--- STATUS ---");
        Console.WriteLine($"Server       : {state.CurrentServer}");
        Console.WriteLine($"Directory    : {state.CurrentDir}");
        Console.WriteLine($"Level        : {state.Level}");
        Console.WriteLine($"Score        : {state.Score}");

        string keys = state.FoundKeys.Count > 0 ? string.Join(", ", state.FoundKeys) : "—";
        Console.WriteLine($"Found keys   : {keys}");





        if (state.UnlockedFiles.Count > 0)
            Console.WriteLine($"Decrypted    : {string.Join(", ", state.UnlockedFiles)}");
        else
            Console.WriteLine($"Decrypted    : —");

    }

    private List<Server> GetVisibleServers()
    {
        return data.Servers.Values
            .Where(server => server.VisibleFromLevel <= state.Level).ToList();
    }

    private void Scan()
    {

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green"))
            .Start("[green]Scanning network...[/]", ctx =>
            {
                ctx.Status("Enumerating hosts...");
                Thread.Sleep(1000);

                ctx.Status("Checking open ports...");
                Thread.Sleep(1000);

                ctx.Status("Analyzing vulnerabilities...");
                Thread.Sleep(1000);

                ctx.Status("Compiling report...");
                Thread.Sleep(1000);
            });

        AnsiConsole.Markup($"\n[green]Servers:[/] ");
        foreach (var server in GetVisibleServers())
        {
            AnsiConsole.Markup($"\n[green]{server.Address}[/] ");
        }

    }

    private void Hack(string arg)
    {
        var servers = data.Servers.Values
            .Where(server => server.VisibleFromLevel <= state.Level).ToList();

        foreach (var server in servers)
        {
            if(server.Locked)
                if (!state.HackedServers.Contains(arg))
                {
                    var password = Console.ReadLine();
                    if (server.Password == password)
                    {
                        state.HackedServers.Add(arg);
                        
                    }
                }

        }
    }
    
    
    private void EndGameMessage()
    {
        Console.Clear();
        AnsiConsole.WriteLine("[green] END GAME");
    }

    private void CheckLevelUp()
    {
        if (state.Score == 4)
            state.Level = 2;
        if (state.Score == 8)
            state.Level = 3;
        if (state.Score == 12)
            EndGameMessage();
    }

public void executeCommand(string command, string arg)
    {
         
                

            switch (command)
            {
                case "ls":
                    ListFiles();break;
                case "cd":
                    ChangeDir(arg);break;
                case "cat":
                    OpenFile(arg);break;
                case "decrypt":
                    Decrypt(arg);break;
                case "hack":
                    Hack(arg); break;
                case "scan":
                    Scan();
                    break;
                case "connect":
                    Connect(arg);
                    break;
                case "status":
                    ShowStatus();break;
                case "clear":
                    Console.Clear();
                    break;
                case "exit":
                    running = false;
                    break;
                case "help":
                    SlowInput("- help, показать список доступных команд\n- ls, показать файлы и папки в текущей директории\n- cd <папка>, перейти в папку, cd .. вернуться назад\n- cat <файл>, прочитать содержимое файла\n- decrypt <файл>, расшифровать зашифрованный файл, нужен ключ или пароль\n- hack <цель>, запустить взлом, мини-игра или ввод пароля\n- scan, просканировать систему, найти подсказки и адреса\n- connect <адрес>, подключиться к следующей системе\n- status, показать прогресс, найденные ключи, очки\n- clear, очистить экран\n- exit, выйти из игры\n\n");
                    break;
                default:
                    AnsiConsole.MarkupLine($"[red]Unknown command: {command}[/]");
                    break;


            }
    }
}