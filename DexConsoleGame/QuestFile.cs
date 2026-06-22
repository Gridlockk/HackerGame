public class QuestData
{
    public string StartServer { get; set; }
    public Dictionary<string, Server> Servers { get; set; }
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