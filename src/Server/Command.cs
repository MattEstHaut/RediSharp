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
            "SET" => new SetCommand(db),
            "GET" => new GetCommand(db),
            "DEL" => new DelCommand(db),
            "LOCK" => new LockCommand(db),
            "UNLOCK" => new UnlockCommand(db),
            "TTL" => new TTLCommand(db),
            "APPEND" => new AppendCommand(db),
            "POP" => new PopCommand(db),
            "TAIL" => new TailCommand(db),
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

public class GetCommand : Command
{
    public GetCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        if (args.Length != 1)
            return new SimpleError("Expected 1 argument");

        var value = _db.Get(args[0]);
        return value == null ? new Null() : new BulkString(value);
    }
}

public class DelCommand : Command
{
    public DelCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        if (args.Length != 1)
            return new SimpleError("Expected 1 argument");

        _db.Del(args[0]);
        return new SimpleString("OK");
    }
}

public class LockCommand : Command
{
    public LockCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        if (args.Length != 1)
            return new SimpleError("Expected 1 argument");

        _db.Lock();
        var mut = _db.Get(args[0]);
        if (mut == null) _db.Set(args[0], "locked");
        _db.Unlock();

        if (mut == null) return new SimpleString("OK");
        return new SimpleError("Key is already locked");
    }
}

public class UnlockCommand : Command
{
    public UnlockCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        if (args.Length != 1)
            return new SimpleError("Expected 1 argument");

        _db.Del(args[0]);

        return new SimpleString("OK");
    }
}

public class TTLCommand : Command
{
    public TTLCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        if (args.Length != 1)
            return new SimpleError("Expected 1 argument");

        if (_db.TTL(args[0]) is not DateTimeOffset ttl)
            return new Null();

        var ms = (ttl - DateTimeOffset.Now).TotalMilliseconds;
        return new Integer((int)ms);
    }
}

public class AppendCommand : Command
{
    public AppendCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        if (args.Length != 2)
            return new SimpleError("Expected 2 arguments");

        _db.Lock();
        var value = _db.Get(args[0]) ?? "";
        value += args[1];
        _db.SetValue(args[0], value);
        _db.Unlock();

        return new Integer(value.Length);
    }
}

public class PopCommand : Command
{
    public PopCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        if (args.Length != 2)
            return new SimpleError("Expected 2 argument");

        if (!int.TryParse(args[1], out int count))
            return new SimpleError("Expected integer as second argument");

        if (count < 0)
            return new SimpleError("Expected non-negative integer as second argument");

        _db.Lock();
        if (_db.Get(args[0]) is not string value)
        {
            _db.Unlock();
            return new Null();
        }

        if (count >= value.Length) count = value.Length;

        var result = value.Substring(0, count);
        value = value.Substring(count);
        _db.SetValue(args[0], value);
        _db.Unlock();

        return new BulkString(result);
    }
}

public class TailCommand : Command
{
    public TailCommand(Database db) : base(db) { }

    public override Item execute(params string[] args)
    {
        if (args.Length != 2)
            return new SimpleError("Expected 2 argument");

        if (!int.TryParse(args[1], out int count))
            return new SimpleError("Expected integer as second argument");

        if (count < 0)
            return new SimpleError("Expected non-negative integer as second argument");

        _db.Lock();
        if (_db.Get(args[0]) is not string value)
        {
            _db.Unlock();
            return new Null();
        }

        if (count >= value.Length) count = value.Length;

        var result = value.Substring(value.Length - count);
        value = value.Substring(0, value.Length - count);
        _db.SetValue(args[0], value);
        _db.Unlock();

        return new BulkString(result);
    }
}