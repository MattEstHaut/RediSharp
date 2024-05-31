int port = 6379;

if (args.Length > 0) port = int.Parse(args[0]);

var database = new Database(15000);
var server = new Server(port, database);
server.Run();