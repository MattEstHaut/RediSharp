using Shared.Resp;

namespace Commands;

public abstract class Command
{
  protected readonly Database _db;

  public Command(Database db) { _db = db; }

  public abstract Item execute(params string[] args);
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