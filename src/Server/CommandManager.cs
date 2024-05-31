using System.Collections.Concurrent;
using Shared.Resp;
using Commands;

public class CommandManager
{
  private class Request
  {
    private readonly Command _command;
    private readonly string[] _args;
    private readonly TaskCompletionSource<Item> _tcs = new();
    public Item Result { get => _tcs.Task.Result; }

    public Request(Command command, string[] args)
    {
      _command = command;
      _args = args;
    }

    public void Execute() => _tcs.SetResult(_command.execute(_args));
  }

  private readonly Database _db;
  private readonly BlockingCollection<Request> _queue = new();
  private CancellationTokenSource _cts = new();

  public CommandManager(Database db) => _db = db;

  public Item Execute(string command, params string[] args)
  {
    var cmd = Command.Create(command, _db);
    var request = new Request(cmd, args);
    _queue.Add(request);
    return request.Result;
  }

  public void Start()
  {
    _cts = new();

    while (!_cts.Token.IsCancellationRequested)
    {
      try { _queue.Take().Execute(); }
      catch (Exception e) { Console.Error.WriteLine(e.Message); }
    }
  }

  public void Stop() => _cts.Cancel();

  public async Task StartAsync() => await Task.Run(() => Start());
}