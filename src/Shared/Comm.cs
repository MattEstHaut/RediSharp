using System.Net.Sockets;

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