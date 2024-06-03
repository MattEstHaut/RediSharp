public class ArgParser
{
    public int Port { get; private set; } = 6379;
    public string? Path { get; private set; } = null;
    public int SaveInterval { get; private set; } = 5 * 60_000;

    public ArgParser(string[] args)
    {
        List<string> arguments = new(args);

        int port = arguments.IndexOf("--port");
        if (port >= 0) Port = int.Parse(arguments[port + 1]);

        int path = arguments.IndexOf("--path");
        if (path >= 0) Path = arguments[path + 1];

        int save = arguments.IndexOf("--save-interval");
        if (save >= 0) SaveInterval = int.Parse(arguments[save + 1]);
    }
}