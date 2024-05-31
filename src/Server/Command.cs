using Shared.Resp;

namespace Commands;

public abstract class Command
{
  protected readonly Database _db;

  public Command(Database db) { _db = db; }

  public abstract Item execute(params string[] args);
}