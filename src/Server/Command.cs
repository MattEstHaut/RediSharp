using Shared.Resp;

namespace Commands;

public abstract class Command
{
    protected readonly Database _db;

    public Command(Database db) { _db = db; }

    public abstract Item execute(params string[] args);

    public static Command Create(string command, Database db)
    {
        return command.ToUpper() switch
        {
            "PING" => new PingCommand(db),
            "ECHO" => new EchoCommand(db),
            _ => new UnknownCommand(db),
        };
    }
}

public class PingCommand : Command
{
    public PingCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        if (args.Length > 0)
            return new SimpleError("No arguments expected");

        return new SimpleString("PONG");
    }
}

public class EchoCommand : Command
{
    public EchoCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        if (args.Length != 1)
            return new SimpleError("Expected 1 argument");

        return new SimpleString(args[0]);
    }
}

public class ErrorCommand : Command
{
    public ErrorCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        return new SimpleError("Unknown error");
    }
}

public class UnknownCommand : Command
{
    public UnknownCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        return new SimpleError("Unknown command");
    }
}

public class SetCommand : Command
{
    public SetCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        switch (args.Length)
        {
            case 2:
                _db.Set(args[0], args[1]);
                return new SimpleString("OK");
            case 4:
                if (args[2].ToUpper() != "EX")
                    return new SimpleError("Expected EX keyword");
                if (!int.TryParse(args[3], out int ms))
                    return new SimpleError("Expected integer after EX keyword");
                _db.Set(args[0], args[1], ms);
                return new SimpleString("OK");
            default:
                return new SimpleError("Expected 2 or 4 arguments");
        }
    }
}