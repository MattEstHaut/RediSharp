using System.Net.Sockets;
using Shared.Resp;

namespace Shared.Comm;

public abstract class Base : IDisposable
{
  protected readonly TcpClient _socket;
  protected readonly StreamReader _reader;
  protected readonly StreamWriter _writer;

  public Base(TcpClient socket)
  {
    _socket = socket;
    var stream = socket.GetStream();
    _reader = new StreamReader(stream);
    _writer = new StreamWriter(stream) { AutoFlush = true };
  }

  public void Dispose()
  {
    _reader.Dispose();
    _writer.Dispose();
    _socket.Dispose();
    GC.SuppressFinalize(this);
  }
}

public class Server : Base
{
  public Server(TcpClient socket) : base(socket) { }

  public Item Send(params string[] command)
  {
    _writer.Write(((ItemArray)command).Encode());
    return Item.Decode(_reader);
  }
}