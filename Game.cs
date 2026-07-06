namespace DexConsoleGame;
using Spectre.Console;
using System.Text.Json;

public class Game
{
    
    private QuestData data;

    private bool running = false;
    private GameState state;
    
    private const int Level2Score = 1;
    private const int Level3Score = 2;
    private const int EndGameScore = 4;

    public Game()
    {
        data = QuestData.Load();
        state = GameState.Load();
    }
    

    public void Run()
    {
        WelcomeMessage();

        running = true;
        while (running)
        {
            try
            {
                AnsiConsole.Markup($"\n[green]root@{state.CurrentServer} > [/] ");

                string input = (Console.ReadLine() ?? "").Trim(); // ?? на случай пустого ввода


                string[] parts = input.Split(' ', 2);
                string command = parts[0].ToLower();
                string arg = parts.Length > 1 ? parts[1] : "";
                ExecuteCommand(command, arg);
                state.Save();
            }
            catch(Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка: {Markup.Escape(ex.Message)}[/]");
            }
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
            Thread.Sleep(20);
        }

        Console.ResetColor();

    }

    private void WelcomeMessage()
    {
        
        string banner = @"
          _____                    _____                    _____                    _____          
         /\    \                  /\    \                  /\    \                  /\    \         
        /::\____\                /::\    \                /::\    \                /::\____\        
       /:::/    /               /::::\    \              /::::\    \              /:::/    /        
      /:::/    /               /::::::\    \            /::::::\    \            /:::/    /         
     /:::/    /               /:::/\:::\    \          /:::/\:::\    \          /:::/    /          
    /:::/____/               /:::/__\:::\    \        /:::/  \:::\    \        /:::/____/           
   /::::\    \              /::::\   \:::\    \      /:::/    \:::\    \      /::::\    \           
  /::::::\    \   _____    /::::::\   \:::\    \    /:::/    / \:::\    \    /::::::\____\________  
 /:::/\:::\    \ /\    \  /:::/\:::\   \:::\    \  /:::/    /   \:::\    \  /:::/\:::::::::::\    \ 
/:::/  \:::\    /::\____\/:::/  \:::\   \:::\____\/:::/____/     \:::\____\/:::/  |:::::::::::\____\
\::/    \:::\  /:::/    /\::/    \:::\  /:::/    /\:::\    \      \::/    /\::/   |::|~~~|~~~~~     
 \/____/ \:::\/:::/    /  \/____/ \:::\/:::/    /  \:::\    \      \/____/  \/____|::|   |          
          \::::::/    /            \::::::/    /    \:::\    \                    |::|   |          
           \::::/    /              \::::/    /      \:::\    \                   |::|   |          
           /:::/    /               /:::/    /        \:::\    \                  |::|   |          
          /:::/    /               /:::/    /          \:::\    \                 |::|   |          
         /:::/    /               /:::/    /            \:::\    \                |::|   |          
        /:::/    /               /:::/    /              \:::\____\               \::|   |          
        \::/    /                \::/    /                \::/    /                \:|   |          
         \/____/                  \/____/                  \/____/                  \|___|   ";
        AnsiConsole.Write(new Markup($"[green]{banner}[/]\n"));
        
        
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
            AnsiConsole.Markup($"[red]Server not found! [/] ");
            return;
        }

        if (relative.Value.Locked && !state.HackedServers.Contains(arg))
        {
            AnsiConsole.Markup($"[red]Connection blocked - server secured [/] ");
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
        if (!data.Servers.TryGetValue(state.CurrentServer, out var server))
        {
            state.CurrentServer = data.StartServer; 
            server = data.Servers[state.CurrentServer];
            state.Save(); 
        }
        return server;
    }

    
    private void ListFiles()
    {
        AnsiConsole.MarkupLine($"[blue]Directory: {state.CurrentDir}[/]");
        for (int i = 0; i < 20; i++)
        {
            AnsiConsole.Write("-");
        }
        AnsiConsole.WriteLine();
        var files = GetVisibleFiles();
        var folders = GetVisibleFolders();
        
        foreach (var folder in folders)
            AnsiConsole.MarkupLine($"[blue]{Markup.Escape(folder)}/[/]");

        foreach (var file in files)
        {
            string name = file.Path.Split('/').Last();
            if (file.Encrypted && !state.UnlockedFiles.Contains(file.Path))
                AnsiConsole.MarkupLine($"[red][[ENC]] {Markup.Escape(name)}[/]");
            else
                AnsiConsole.WriteLine(Markup.Escape(name));
        }
    }
    
    private void ChangeDir(string arg)
    {
        
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
            return;
        }

        var folders = GetVisibleFolders();

        if (!folders.Contains(arg))
        {
            AnsiConsole.MarkupLine($"[red]Path unknown: {Markup.Escape(arg)}[/]");
            return;
        }
        
     

        if (state.CurrentDir == "/")
        {
            state.CurrentDir = "/" + arg;
        }
        else
        {
            state.CurrentDir = state.CurrentDir + "/" + arg;
        }

    }
    private List<string> GetVisibleFolders()
    {
        var files = GetCurrentServer().Files
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

            if (parts.Length > 1)
            {
                folders.Add(parts[0]);
            }
        }

        return folders.ToList();
    }
    private List<QuestFile> GetVisibleFiles()
    {
        return GetCurrentServer().Files
            .Where(f => f.VisibleFromLevel <= state.Level)
            .Where(f =>
            {
                string dir = state.CurrentDir == "/" ? "/" : state.CurrentDir + "/";

                if (dir == "/")
                {
                    return !f.Path.Substring(1).Contains("/");
                }

                if (!f.Path.StartsWith(dir))
                    return false;

                string relative = f.Path.Substring(dir.Length);

                return !relative.Contains("/");
            })
            .ToList();
    }
    
    private void OpenFile(string arg)
    {
        var file = GetVisibleFiles()
            .FirstOrDefault(f => f.Path.Split('/').Last() == arg);
    
        if (file == null)
        {
            AnsiConsole.MarkupLine("[red]File not found[/]");
            return;
        }
    
        if (file.Encrypted)
        {
            if (!state.UnlockedFiles.Contains(file.Path))
            {
                AnsiConsole.MarkupLine("[red]File encrypted!!![/]");
                return;
            }
        
            var decrypted = Cipher.Decrypt(file.Content, file.Key ?? 0);
            AnsiConsole.MarkupLine($"[blue]{Markup.Escape(decrypted)}[/]");
            return;
        }
    
        AnsiConsole.MarkupLine($"[blue]{Markup.Escape(file.Content)}[/]");
    }

 

    private void Decrypt(string arg)
    {
        var file = GetVisibleFiles()
            .FirstOrDefault(f => f.Path.Split('/').Last() == arg);
    
        if (file == null)
        {
            AnsiConsole.MarkupLine("[red]File not found[/]");
            return;
        }

        if (!file.Encrypted)
        {
            AnsiConsole.MarkupLine("[yellow]This file is not encrypted. Use 'cat' to read it.[/]");
            return;
        }

        if (state.UnlockedFiles.Contains(file.Path))
        {
            AnsiConsole.MarkupLine("[yellow]Already decrypted. Use 'cat' to view content.[/]");
            return;
        }
        AnsiConsole.Markup("[green]Input Key > [/]");
        if (!int.TryParse(Console.ReadLine(), out int keyDec))
        {
            AnsiConsole.MarkupLine("[red]Invalid key format! Please enter a number.[/]");
            return;
        }
        if (file.Key != keyDec)   
        {
            AnsiConsole.MarkupLine("[red]Wrong key![/]");
            return;
        }
        state.UnlockedFiles.Add(file.Path);
        state.FoundKeys.Add(keyDec.ToString());
        state.Score++;
        CheckLevelUp();
        if (!running) return;
        var decrypted = Cipher.Decrypt(file.Content, keyDec);
        AnsiConsole.MarkupLine($"[blue]{Markup.Escape(decrypted)}[/]");
        
    }


    

    private void ShowStatus()
    {
        var table = new Table();

        table.Border(TableBorder.Rounded);
        table.AddColumn("Property");
        table.AddColumn("Value");

        table.AddRow("Server", state.CurrentServer);
        table.AddRow("Directory", state.CurrentDir);
        table.AddRow("Level", state.Level.ToString());
        table.AddRow("Score", state.Score.ToString());

        table.AddRow(
            "Found Keys",
            state.FoundKeys.Count > 0
                ? string.Join(" ", state.FoundKeys)
                : "-"
        );

        table.AddRow(
            "Unlocked Files",
            state.UnlockedFiles.Count > 0
                ? string.Join(" ", state.UnlockedFiles)
                : "-"
        );

        table.AddRow(
            "Hacked Servers",
            state.HackedServers.Count > 0
                ? string.Join(" ", state.HackedServers)
                : "-"
        );

        AnsiConsole.Write(table);
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
            if (server.Locked && !state.HackedServers.Contains(server.Address))
                AnsiConsole.Markup($"\n[green]{server.Address} \t STATUS: [/][red]PROTECTED[/] ");
            else if(server.Locked && state.HackedServers.Contains(server.Address))
                AnsiConsole.Markup($"\n[green]{server.Address} \t STATUS: [/][green]Hacked[/] ");
            else
                AnsiConsole.Markup($"\n[green]{server.Address} \t STATUS: OPEN[/] ");
        }
        var hint = data.Hints?
                .FirstOrDefault(h => h.VisibleFromLevel == state.Level)
            ;
        
        if (hint != null)
        {
            AnsiConsole.MarkupLine($"\n\n[yellow]Hint: {Markup.Escape(hint.Text)}[/]");
            
        }

    }

    private void Hack(string arg)
    {
       
        var server = data.Servers.Values
            .FirstOrDefault(server => server.VisibleFromLevel <= state.Level && server.Address == arg);

        if (server != null)
        {
            if (server.Locked)
            {
                if (!state.HackedServers.Contains(arg))
                {
                    AnsiConsole.Markup($"\n[green]Enter password: > [/] ");
                    var password = Console.ReadLine();
                    

                    AnsiConsole.Progress()
                        .Start(ctx =>
                        {
                            var task = ctx.AddTask("Hacking....", maxValue: 100);

                            while (!ctx.IsFinished)
                            {
                                task.Increment(1);

                                Thread.Sleep(10);
                            }
                        });
                    
                    if (server.Password == password)
                    {
                        AnsiConsole.Markup($"[green]  Hack successful [/]");

                        state.HackedServers.Add(arg);
                    }
                    else
                    {
                        AnsiConsole.Markup($"[red]  Access deny - Incorrect password [/]");
                    }
                }
                else
                {
                    AnsiConsole.Markup($"[red]  Server has been hacked before [/]");
                    
                }
            }
            else
            {
                AnsiConsole.Markup($"[red]  Server has no password [/]");
            }
        }
        else
        {
            AnsiConsole.Markup($"[red]  Server not found[/]");
        }
    }
    
    
    private void EndGameMessage()
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[green]END GAME[/]");
        running = false;
    }

    private void CheckLevelUp()
    {
        if (state.Score == Level2Score)
            state.Level = 2;
        else if (state.Score == Level3Score)
            state.Level = 3;
        else if (state.Score == EndGameScore)
            EndGameMessage();
    }

public void ExecuteCommand(string command, string arg)
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
                    AnsiConsole.MarkupLine($"[red]Unknown command: {Markup.Escape(command)}[/]");
                    break;


            }
    }
}