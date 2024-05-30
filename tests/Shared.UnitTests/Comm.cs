using System.Net.Sockets;
using Shared.Comm;
using Shared.Resp;

namespace Shared.unitTests;

public class CommTests
{
  [Theory]
  [InlineData()]
  [InlineData("")]
  [InlineData("PING")]
  [InlineData("GET", "key")]
  [InlineData("SET", "key", "value")]
  public async Task SendCommand(params string[] command)
  {
    using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
    listener.Start();
    int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;

    var serverTask = listener.AcceptTcpClientAsync();
    using var server = new Server(new TcpClient("localhost", port));
    using var client = new Client(await serverTask);

    var responseTask = Task.Run(() => server.Send(command));
    var received = client.Read();
    client.Send(received);

    var response = await responseTask;

    Assert.Equal(command.Length, ((ItemArray)received).Items.Count);
    Assert.Equal(command, ((ItemArray)received).Items.Select(i => i.ToString()));
    Assert.Equal(response.ToString(), received.ToString());
  }
}