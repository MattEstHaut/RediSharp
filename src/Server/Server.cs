using System.Net;
using System.Net.Sockets;

public class Server
{
  private readonly TcpListener _listener;
  private readonly ConnectionHandler _handler;
  private readonly CommandManager _manager;

  public Server(int port, Database db)
  {
    _listener = new TcpListener(IPAddress.Any, port);
    _manager = new CommandManager(db);
    _handler = new ConnectionHandler(_manager);
  }

  public void Run()
  {
    _listener.Start();
    _ = _manager.StartAsync();

    while (_listener.Server.IsBound)
    {
      var socket = _listener.AcceptTcpClient();
      _ = _handler.HandleAsync(socket);
    }
  }

  public void Stop()
  {
    _listener.Stop();
    _manager.Stop();
  }

  public void RunAsync() => Task.Run(() => Run());
}