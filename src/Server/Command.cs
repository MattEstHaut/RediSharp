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
      _ => new ErrorCommand(db),
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