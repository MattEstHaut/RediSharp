var arguments = new ArgParser(args);

var database = Database.Link(arguments.Path, arguments.SaveInterval);
var server = new Server(arguments.Port, database);

server.Run();