using System.Net.Sockets;
using Shared.Comm;
using Shared.Resp;

public class ConnectionHandler
{
    private readonly CommandManager _manager;

    public ConnectionHandler(CommandManager manager)
    {
        _manager = manager;
    }

    public void Handle(TcpClient socket)
    {
        using var client = new Client(socket);

        try
        {
            while (socket.Connected)
            {
                var request = client.Read();

                if (request is not ItemArray array)
                    throw new Exception("Expected array");

                var args = array.Items.ConvertAll(i => i.ToString() ?? "").ToArray();

                if (args.Length == 0)
                    throw new Exception("Expected at least 1 argument");

                var command = args[0];
                var response = _manager.Execute(command, args[1..]);
                client.Send(response);
            }
        }
        finally { }
    }

    public async Task HandleAsync(TcpClient socket)
    {
        await Task.Run(() => Handle(socket));
    }
}