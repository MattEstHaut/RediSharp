using System.Net.Sockets;
using Shared.Comm;

public class Client
{
  private readonly Server _server;

  public Client(TcpClient socket) => _server = new Server(socket);

  public void Run()
  {
    while (CLI.Read(out var command))
    {
      var tokens = CLI.Tokenize(command);
      var response = _server.Send(tokens);
      CLI.Write(response);
    }
  }
}