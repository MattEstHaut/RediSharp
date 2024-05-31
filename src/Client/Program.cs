using System.Net.Sockets;

string host = "localhost";
int port = 6379;

if (args.Length > 0) host = args[0];
if (args.Length > 1) port = int.Parse(args[1]);

using var socket = new TcpClient(host, port);

var client = new Client(socket);
client.Run();