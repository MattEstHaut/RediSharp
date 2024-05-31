using System.Text;
using System.Text.RegularExpressions;
using Shared.Resp;

public static class CLI
{
  public static bool Read(out string command)
  {
    Console.Write("> ");
    var line = Console.ReadLine();
    command = line ?? "";
    return line != null;
  }

  public static void Write(Item item)
  {
    if (item is SimpleError) Console.ForegroundColor = ConsoleColor.Red;
    if (item is Null) Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine(item);
    Console.ResetColor();
  }

  public static string[] Tokenize(string command)
  {
    var tokens = new List<string>();
    var queue = new Queue<char>(command);

    while (queue.Count > 0)
    {
      switch (queue.Peek())
      {
        case ' ':
          queue.Dequeue();
          break;
        case '"':
        case '\'':
          tokens.Add(ReadString(queue));
          break;
        default:
          tokens.Add(ReadKeyword(queue));
          break;
      }
    }

    return tokens.ToArray();
  }

  private static string ReadKeyword(Queue<char> command)
  {
    var keyword = new StringBuilder();
    while (command.Count > 0 && command.Peek() != ' ')
      keyword.Append(command.Dequeue());
    return keyword.ToString();
  }

  private static string ReadString(Queue<char> command)
  {
    var str = new StringBuilder();
    var quote = command.Dequeue();
    bool escaped = false;

    while (command.Count > 0)
    {
      var c = command.Dequeue();
      if (escaped)
      {
        str.Append(c);
        escaped = false;
      }
      else if (c == '\\') escaped = true;
      else if (c == quote) break;
      else
      {
        str.Append(c);
        escaped = false;
      }
    }

    return str.ToString();
  }
}